using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Domain;
using UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection;
using UberDeployer.Core.TeamCity;
using UberDeployer.Core.TeamCity.ApiModels;

namespace UberDeployer.Core.Deployment.Tasks
{
  public class DeployDependenciesTask : DeploymentTaskBase
  {
    private readonly string _projectName;
    private readonly string _targetEnvironment;
    private readonly string _defaultTeamCityProjectConfiguration;
    private readonly IProjectInfoRepository _projectInfoRepository;
    private readonly IObjectFactory _objectFactory;
    private readonly ITeamCityRestClient _temCityRestClient;
    private readonly IDependentProjectsToDeployWebSelector _dependentProjectsToDeploySelector;

    private readonly List<DeploymentTaskBase> _subTasks;
    private readonly Guid _deploymentId;

    public DeployDependenciesTask(
      string projectName,
      string targetEnvironment,
      Guid deploymentId,
      string defaultTeamCityProjectConfiguration,
      IProjectInfoRepository projectInfoRepository,
      IObjectFactory objectFactory,
      ITeamCityRestClient temCityRestClient,
      IDependentProjectsToDeployWebSelector dependentProjectsToDeploySelector)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");
      Guard.NotNullNorEmpty(targetEnvironment, "targetEnvironment");
      Guard.NotEmpty(deploymentId, "deploymentId");
      Guard.NotNullNorEmpty(defaultTeamCityProjectConfiguration, "defaultBuildConfiguration");
      Guard.NotNull(projectInfoRepository, "projectInfoRepository");
      Guard.NotNull(objectFactory, "objectFactory");
      Guard.NotNull(temCityRestClient, "temCityRestClient");
      Guard.NotNull(dependentProjectsToDeploySelector, "dependentProjectsToDeploySelector");
      
      _projectName = projectName;
      _targetEnvironment = targetEnvironment;
      _deploymentId = deploymentId;
      _defaultTeamCityProjectConfiguration = defaultTeamCityProjectConfiguration;
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
      // TODO MARIO: catch exceptions
      foreach (var subTask in _subTasks)
      {
        subTask.Execute();
      }
    }

    protected override void DoPrepare()
    {
      // TODO MARIO: catch exceptions??
      List<ProjectInfo> dependentProjectsToDeploy = GetDependentProjectsToDeploy(_projectName);

      List<ProjectDeployment> defaultProjectDeployments = BuildDefaultProjectDeployments(dependentProjectsToDeploy, _defaultTeamCityProjectConfiguration);

      List<ProjectDeployment> configuredProjectDeployments = ConfigureDeployments(defaultProjectDeployments);

      foreach (ProjectDeployment projectDeployment in configuredProjectDeployments)
      {
        DeploymentTask deploymentTask = projectDeployment.ProjectInfo.CreateDeploymentTask(_objectFactory);

        deploymentTask.Initialize(projectDeployment.DeploymentInfo);
        deploymentTask.Prepare();

        AddSubTask(deploymentTask);
      }
    }

    private List<ProjectDeployment> BuildDefaultProjectDeployments(IEnumerable<ProjectInfo> dependentProjectsToDeploy, string defaultTeamCityProjectConfiguration)
    {
      var projectDeployments = new List<ProjectDeployment>();

      foreach (var projectInfo in dependentProjectsToDeploy)
      {
        IEnumerable<TeamCityBuildType> teamCityBuildTypes = _temCityRestClient.GetBuildTypes(projectInfo.Name);

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
        
        var deploymentInfo = new DeploymentInfo(_deploymentId, false, projectInfo.Name, defaultTeamCityProjectConfiguration, lastSuccessfulBuild.Id, _targetEnvironment, null, false);

        projectDeployments.Add(
          new ProjectDeployment
          {
            ProjectInfo = projectInfo,
            DeploymentInfo = deploymentInfo,
          });
      }

      return projectDeployments;
    }

    public class ProjectDeployment
    {
      public ProjectInfo ProjectInfo { get; set; }

      public DeploymentInfo DeploymentInfo { get; set; }
    }

    private List<ProjectDeployment> ConfigureDeployments(List<ProjectDeployment> defaultDeploymentInfos)
    {
      throw new NotImplementedException();
    }

    private List<DeploymentInfo> BuildDefaultDeploymentInfos(List<ProjectInfo> dependentProjectsToDeploy, string defaultTeamCityProjectConfiguration)
    {
      throw new NotImplementedException();
    }    

    private void GetLatestBuildForProjects(List<ProjectInfo> dependentProjectsToDeploy, string defaultTeamCityProjectConfiguration)
    {
      throw new NotImplementedException();
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

    private List<ProjectInfo> GetDependentProjectsToDeploy(string projectName)
    {
      throw new NotImplementedException();
    }
  }
}