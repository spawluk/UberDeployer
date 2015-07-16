using System;
using System.Collections.Generic;
using System.Linq;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Domain;
using UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection;
using UberDeployer.Core.TeamCity;
using UberDeployer.Core.TeamCity.ApiModels;

namespace UberDeployer.Core.Deployment.Tasks.DependenciesDeployment
{
  public class DeployDependenciesTask : DeploymentTaskBase
  {
    private readonly string _projectName;
    private readonly string _targetEnvironment;
    private readonly IProjectInfoRepository _projectInfoRepository;
    private readonly IObjectFactory _objectFactory;
    private readonly ITeamCityRestClient _temCityRestClient;
    private readonly IDependentProjectsToDeployWebSelector _dependentProjectsToDeploySelector;

    private readonly List<DeploymentTaskBase> _subTasks;
    private readonly Guid _deploymentId;

    private const string _DefaultTeamCityProjectConfiguration = "Production";

    public DeployDependenciesTask(
      string projectName,
      string targetEnvironment,
      Guid deploymentId,
      IProjectInfoRepository projectInfoRepository,
      IObjectFactory objectFactory,
      ITeamCityRestClient temCityRestClient,
      IDependentProjectsToDeployWebSelector dependentProjectsToDeploySelector)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");
      Guard.NotNullNorEmpty(targetEnvironment, "targetEnvironment");
      Guard.NotEmpty(deploymentId, "deploymentId");
      Guard.NotNull(projectInfoRepository, "projectInfoRepository");
      Guard.NotNull(objectFactory, "objectFactory");
      Guard.NotNull(temCityRestClient, "temCityRestClient");
      Guard.NotNull(dependentProjectsToDeploySelector, "dependentProjectsToDeploySelector");
      
      _projectName = projectName;
      _targetEnvironment = targetEnvironment;
      _deploymentId = deploymentId;
      _projectInfoRepository = projectInfoRepository;
      _objectFactory = objectFactory;
      _temCityRestClient = temCityRestClient;
      _dependentProjectsToDeploySelector = dependentProjectsToDeploySelector;

      _subTasks = new List<DeploymentTaskBase>();
    }

    public override string Description
    {
      get { return string.Format("Deploys all dependencies for project: [{0}]", _projectName); }
    }

    protected override void DoExecute()
    {
      foreach (var subTask in _subTasks)
      {
        try
        {
          subTask.Execute();
        }
        catch (Exception exc)
        {
          PostDiagnosticMessage(
            string.Format("Error while executing task: [{0}] with description: [{1}], exception: [{2}]", subTask.GetType().FullName, subTask.Description, exc), 
            DiagnosticMessageType.Error);
        }
      }
    }

    protected override void DoPrepare()
    {
      List<ProjectInfo> dependentProjectsToDeploy = _projectInfoRepository.CreateDependentProjects(_projectName);

      if (!dependentProjectsToDeploy.Any())
      {
        PostDiagnosticMessage(string.Format("No dependent projects to deploy for project: [{0}]", _projectName), DiagnosticMessageType.Trace);
        return;
      }

      List<ProjectDeployment> defaultProjectDeployments = BuildDefaultProjectDeployments(dependentProjectsToDeploy, _DefaultTeamCityProjectConfiguration);

      IEnumerable<ProjectDeployment> configuredProjectDeployments = ConfigureDeploymentsByClient(defaultProjectDeployments);

      foreach (ProjectDeployment projectDeployment in configuredProjectDeployments)
      {
        DeploymentTask deploymentTask = projectDeployment.ProjectInfo.CreateDeploymentTask(_objectFactory);

        try
        {
          deploymentTask.Initialize(projectDeployment.DeploymentInfo);
          deploymentTask.Prepare();

          AddSubTask(deploymentTask);
        }
        catch (Exception exc)
        {
          PostDiagnosticMessage(
            string.Format("Error while preparing task: [{0}] with description: [{1}], exception: [{2}]", deploymentTask.GetType().FullName, deploymentTask.Description, exc),
            DiagnosticMessageType.Error);
        }
      }
    }

    private List<ProjectDeployment> BuildDefaultProjectDeployments(IEnumerable<ProjectInfo> dependentProjectsToDeploy, string defaultTeamCityProjectConfiguration)
    {
      var projectDeployments = new List<ProjectDeployment>();

      foreach (var projectInfo in dependentProjectsToDeploy)
      {
        IEnumerable<TeamCityBuildType> teamCityBuildTypes = _temCityRestClient.GetBuildTypes(projectInfo.ArtifactsRepositoryName);

        TeamCityBuildType defaultBuildType = teamCityBuildTypes.FirstOrDefault(x => x.Name == defaultTeamCityProjectConfiguration);

        if (defaultBuildType == null)
        {
          throw new DeploymentTaskException(string.Format("TeamCity configuration: [{0}] does not exist for project: [{1}]", defaultTeamCityProjectConfiguration, projectInfo.Name));
        }

        TeamCityBuild lastSuccessfulBuild = _temCityRestClient.GetLastSuccessfulBuild(defaultBuildType.Id);

        if (lastSuccessfulBuild == null)
        {
          throw new DeploymentTaskException(string.Format("Cannot obtain last successful build for project [{0}], configuration: [{1}], team city build type id: [{2}]", projectInfo.Name, defaultTeamCityProjectConfiguration, defaultBuildType.Id));
        }

        var deploymentInfo = new DeploymentInfo(_deploymentId, false, projectInfo.Name, defaultTeamCityProjectConfiguration, lastSuccessfulBuild.Id, _targetEnvironment, projectInfo.CreateEmptyInputParams());

        projectDeployments.Add(
          new ProjectDeployment
          {
            ProjectInfo = projectInfo,
            DeploymentInfo = deploymentInfo,
            TeamCityBuild = lastSuccessfulBuild,
          });
      }

      return projectDeployments;
    }      

    private IEnumerable<ProjectDeployment> ConfigureDeploymentsByClient(List<ProjectDeployment> defaultDeploymentInfos)
    {      
      List<DependentProject> dependentProjects = ConvertToDependentProjects(defaultDeploymentInfos);

      DependentProjectsToDeploySelection dependentProjectsToDeploySelection = _dependentProjectsToDeploySelector.GetSelectedProjectsToDeploy(_deploymentId, dependentProjects);

      return FilterBySelectedProjects(defaultDeploymentInfos, dependentProjectsToDeploySelection.SelectedProjects);
    }

    private static IEnumerable<ProjectDeployment> FilterBySelectedProjects(IEnumerable<ProjectDeployment> defaultDeploymentInfos, IEnumerable<DependentProject> selectedProjects)
    {
      return
        selectedProjects.Join(
          defaultDeploymentInfos,
          selectedProject => selectedProject.ProjectName,
          defaultProject => defaultProject.ProjectInfo.Name,
          (selectedProject, defaultProject) => defaultProject);
    }

    private List<DependentProject> ConvertToDependentProjects(IEnumerable<ProjectDeployment> defaultDeploymentInfos)
    {
      return defaultDeploymentInfos.Select(
        x => new DependentProject
        {
          BranchName = x.DeploymentInfo.ProjectConfigurationName,
          BuildNumber = x.TeamCityBuild.Number,
          ProjectName = x.ProjectInfo.Name,
        })
        .ToList();
    }

    private void AddSubTask(DeploymentTaskBase subTask)
    {
      if (subTask == null)
      {
        throw new ArgumentNullException("subTask");
      }

      _subTasks.Add(subTask);

      // this will cause the events raised by sub-tasks to bubble up
      subTask.DiagnosticMessagePosted += OnDiagnosticMessagePosted;
    }    
  }
}