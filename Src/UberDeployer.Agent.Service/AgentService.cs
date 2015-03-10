using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using UberDeployer.Agent.Proxy;
using UberDeployer.Agent.Proxy.Dto;
using UberDeployer.Agent.Proxy.Dto.TeamCity;
using UberDeployer.Agent.Proxy.Faults;
using UberDeployer.Agent.Service.Diagnostics;
using UberDeployer.Common;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.CommonConfiguration;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Deployment.Pipeline.Modules;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Domain.Input;
using UberDeployer.Core.Management.Metadata;
using UberDeployer.Core.TeamCity;
using log4net;
using UberDeployer.Core.Deployment.Tasks;
using DeploymentInfo = UberDeployer.Agent.Proxy.Dto.DeploymentInfo;
using DeploymentRequest = UberDeployer.Core.Deployment.Pipeline.Modules.DeploymentRequest;
using DiagnosticMessage = UberDeployer.Core.Deployment.DiagnosticMessage;
using DiagnosticMessageType = UberDeployer.Core.Deployment.DiagnosticMessageType;
using EnvironmentInfo = UberDeployer.Core.Domain.EnvironmentInfo;
using MachineSpecificProjectVersion = UberDeployer.Agent.Proxy.Dto.Metadata.MachineSpecificProjectVersion;
using Project = UberDeployer.Core.TeamCity.Models.Project;
using ProjectInfo = UberDeployer.Core.Domain.ProjectInfo;
using ProjectType = UberDeployer.Core.Domain.ProjectType;
using WebAppProjectInfo = UberDeployer.Core.Domain.WebAppProjectInfo;

namespace UberDeployer.Agent.Service
{
  public class AgentService : IAgentService
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IDeploymentPipeline _deploymentPipeline;
    private readonly IEnvDeploymentPipeline _envDeploymentPipeline;
    private readonly IProjectInfoRepository _projectInfoRepository;
    private readonly IEnvironmentInfoRepository _environmentInfoRepository;
    private readonly IEnvironmentDeployInfoRepository _environmentDeployInfoRepository;
    private readonly ITeamCityClient _teamCityClient; // TODO IMM HI: abstract away?
    private readonly IDeploymentRequestRepository _deploymentRequestRepository;
    private readonly IDiagnosticMessagesLogger _diagnosticMessagesLogger;
    private readonly IProjectMetadataExplorer _projectMetadataExplorer;
    private readonly IDirPathParamsResolver _dirPathParamsResolver;    

    #region Constructor(s)

    public AgentService(
      IDeploymentPipeline deploymentPipeline,
      IProjectInfoRepository projectInfoRepository,
      IEnvironmentInfoRepository environmentInfoRepository,
      ITeamCityClient teamCityClient,
      IDeploymentRequestRepository deploymentRequestRepository,
      IDiagnosticMessagesLogger diagnosticMessagesLogger,
      IProjectMetadataExplorer projectMetadataExplorer,
      IDirPathParamsResolver dirPathParamsResolver, 
      IEnvironmentDeployInfoRepository environmentDeployInfoRepository, 
      IEnvDeploymentPipeline envDeploymentPipeline)
    {
      Guard.NotNull(deploymentPipeline, "deploymentPipeline");
      Guard.NotNull(projectInfoRepository, "projectInfoRepository");
      Guard.NotNull(environmentInfoRepository, "environmentInfoRepository");
      Guard.NotNull(teamCityClient, "teamCityClient");
      Guard.NotNull(deploymentRequestRepository, "deploymentRequestRepository");
      Guard.NotNull(diagnosticMessagesLogger, "diagnosticMessagesLogger");
      Guard.NotNull(dirPathParamsResolver, "dirPathParamsResolver");
      Guard.NotNull(environmentDeployInfoRepository, "environmentDeployInfoRepository");
      Guard.NotNull(envDeploymentPipeline, "envDeploymentPipeline");

      _projectInfoRepository = projectInfoRepository;
      _environmentInfoRepository = environmentInfoRepository;
      _teamCityClient = teamCityClient;
      _deploymentPipeline = deploymentPipeline;
      _deploymentRequestRepository = deploymentRequestRepository;
      _diagnosticMessagesLogger = diagnosticMessagesLogger;
      _projectMetadataExplorer = projectMetadataExplorer;
      _dirPathParamsResolver = dirPathParamsResolver;
      _environmentDeployInfoRepository = environmentDeployInfoRepository;
      _envDeploymentPipeline = envDeploymentPipeline;
    }

    public AgentService()
      : this(
        ObjectFactory.Instance.CreateDeploymentPipeline(),
        ObjectFactory.Instance.CreateProjectInfoRepository(),
        ObjectFactory.Instance.CreateEnvironmentInfoRepository(),
        ObjectFactory.Instance.CreateTeamCityClient(),
        ObjectFactory.Instance.CreateDeploymentRequestRepository(),
        InMemoryDiagnosticMessagesLogger.Instance,
        ObjectFactory.Instance.CreateProjectMetadataExplorer(),
        ObjectFactory.Instance.CreateDirPathParamsResolver(), 
        ObjectFactory.Instance.CreateEnvironmentDeployInfoRepository(), 
        ObjectFactory.Instance.CrateEnvDeploymentPipeline())
    {
    }

    #endregion

    #region IAgentService Members

    public void Deploy(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfoDto)
    {
      try
      {
        Guard.NotEmpty(deploymentId, "deploymentId");
        Guard.NotEmpty(uniqueClientId, "uniqueClientId");
        Guard.NotNullNorEmpty(requesterIdentity, "requesterIdentity");
        Guard.NotNull(deploymentInfoDto, "DeploymentInfo");

        ProjectInfo projectInfo =
          _projectInfoRepository.FindByName(deploymentInfoDto.ProjectName);

        if (projectInfo == null)
        {
          throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = deploymentInfoDto.ProjectName });
        }

        Core.Domain.DeploymentInfo deploymentInfo =
          DtoMapper.ConvertDeploymentInfo(deploymentInfoDto, projectInfo);

        DoDeploy(uniqueClientId, requesterIdentity, deploymentInfo, projectInfo);
      }
      catch (Exception exc)
      {
        HandleDeploymentException(exc, uniqueClientId);
      }
    }

    public void DeployAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfoDto)
    {
      try
      {
        Guard.NotEmpty(deploymentId, "deploymentId");
        Guard.NotEmpty(uniqueClientId, "uniqueClientId");
        Guard.NotNullNorEmpty(requesterIdentity, "requesterIdentity");
        Guard.NotNull(deploymentInfoDto, "deploymentInfo");

        ProjectInfo projectInfo =
          _projectInfoRepository.FindByName(deploymentInfoDto.ProjectName);

        if (projectInfo == null)
        {
          throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = deploymentInfoDto.ProjectName });
        }

        Core.Domain.DeploymentInfo deploymentInfo = DtoMapper.ConvertDeploymentInfo(deploymentInfoDto, projectInfo);

        ThreadPool.QueueUserWorkItem(
          state =>
          {
            try
            {
              DoDeploy(uniqueClientId, requesterIdentity, deploymentInfo, projectInfo);
            }
            catch (Exception exc)
            {
              HandleDeploymentException(exc, uniqueClientId);
            }
          });
      }
      catch (Exception exc)
      {
        HandleDeploymentException(exc, uniqueClientId);
      }
    }

    public void CreatePackageAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfo, string packageDirPath)
    {
      try
      {
        Guard.NotEmpty(deploymentId, "deploymentId");
        Guard.NotEmpty(uniqueClientId, "uniqueClientId");
        Guard.NotNullNorEmpty(requesterIdentity, "requesterIdentity");
        Guard.NotNull(deploymentInfo, "deploymentInfo");

        ProjectInfo projectInfo =
          _projectInfoRepository.FindByName(deploymentInfo.ProjectName);

        if (projectInfo == null)
        {
          throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = deploymentInfo.ProjectName });
        }

        //ThreadPool.QueueUserWorkItem(
        //  state =>
        //  {
            try
            {
              DoCreatePackage(uniqueClientId, requesterIdentity, deploymentInfo, projectInfo, packageDirPath);
            }
            catch (Exception exc)
            {
              HandleDeploymentException(exc, uniqueClientId);
            }
          //});
      }
      catch (Exception exc)
      {
        HandleDeploymentException(exc, uniqueClientId);
      }
    }

    public void DeployEnvironmentAsync(Guid uniqueClientId, string requesterIdentity, string targetEnvironment)
    {
      Guard.NotEmpty(uniqueClientId, "uniqueClientId");
      Guard.NotNullNorEmpty(requesterIdentity, "requesterIdentity");
      Guard.NotNullNorEmpty(targetEnvironment, "targetEnvironment");

      EnvironmentDeployInfo environmentDeployInfo = _environmentDeployInfoRepository.FindByName(targetEnvironment);

      if (environmentDeployInfo == null)
      {
        throw new FaultException<EnvironmentDeployConfigurationNotFoundFault>(new EnvironmentDeployConfigurationNotFoundFault { EnvironmentName = targetEnvironment });
      }

      List<ProjectDeploymentData> projectsToDeploy = CreateProjectDeployments(uniqueClientId, environmentDeployInfo).ToList();

      ThreadPool.QueueUserWorkItem(
        state =>
        {
          EventHandler<DiagnosticMessageEventArgs> deploymentPipelineDiagnosticMessageAction =
            (eventSender, tmpArgs) => LogMessage(uniqueClientId, tmpArgs.MessageType, tmpArgs.Message);

          try
          {
            _envDeploymentPipeline.DiagnosticMessagePosted += deploymentPipelineDiagnosticMessageAction;

            _envDeploymentPipeline.StartDeployment(targetEnvironment, projectsToDeploy, new DeploymentContext(requesterIdentity));
          }
          finally
          {
            _envDeploymentPipeline.DiagnosticMessagePosted -= deploymentPipelineDiagnosticMessageAction;
          }
        });
    }

    private IEnumerable<ProjectDeploymentData> CreateProjectDeployments(Guid uniqueClientId, EnvironmentDeployInfo environmentDeployInfo)
    {
      var projectDeployments = new List<ProjectDeploymentData>();

      EnvironmentInfo environmentInfo = _environmentInfoRepository.FindByName(environmentDeployInfo.TargetEnvironment);

      if (environmentInfo == null)
      {
        throw new FaultException<EnvironmentNotFoundFault>(new EnvironmentNotFoundFault { EnvironmentName = environmentDeployInfo.TargetEnvironment });
      }

      foreach (var projectToDeploy in environmentDeployInfo.ProjectsToDeploy)
      {
        try
        {
          ProjectInfo projectInfo = _projectInfoRepository.FindByName(projectToDeploy);

          if (projectInfo == null)
          {
            throw new DeploymentTaskException(string.Format("Not found configuration for project: {0}", projectToDeploy));
          }

          ProjectConfigurationBuild lastSuccessfulBuild = GetLatestSuccessfulBuild(projectToDeploy, environmentDeployInfo.BuildConfigurationName);

          if (lastSuccessfulBuild == null)
          {
            throw new DeploymentTaskException(string.Format("Successful build not found for project: {0} and configuration: {1}", projectToDeploy, environmentDeployInfo.BuildConfigurationName));
          }

          InputParams inputParams = BuildInputParams(projectInfo, environmentInfo);
          var deploymentInfo = new Core.Domain.DeploymentInfo(Guid.NewGuid(), false, projectToDeploy, environmentDeployInfo.BuildConfigurationName, lastSuccessfulBuild.Id, environmentDeployInfo.TargetEnvironment, inputParams);

          DeploymentTask deploymentTask = CreateDeploymentTask(projectInfo);

          projectDeployments.Add(new ProjectDeploymentData(deploymentInfo, projectInfo, deploymentTask));
        }
        catch (Exception e)
        {
          LogMessage(uniqueClientId, DiagnosticMessageType.Error, e.Message);
        }
      }

      return projectDeployments;
    }

    private static DeploymentTask CreateDeploymentTask(ProjectInfo projectInfo)
    {
      if (projectInfo.Type == ProjectType.Db)
      {
        // for database projects return publish task instead of default db deployment task.
        return new PublishDbProjectDeploymentTask(          
          ObjectFactory.Instance.CreateProjectInfoRepository(),
          ObjectFactory.Instance.CreateEnvironmentInfoRepository(),
          ObjectFactory.Instance.CreateArtifactsRepository(),
          ObjectFactory.Instance.CreateFileAdapter(),
          ObjectFactory.Instance.CreateZipFileAdapter(),
          ObjectFactory.Instance.CreateDbManagerFactory(), 
          ObjectFactory.Instance.CreateMsSqlDatabasePublisher());        
      }

      return projectInfo.CreateDeploymentTask(ObjectFactory.Instance);
    }

    private static InputParams BuildInputParams(ProjectInfo projectInfo, EnvironmentInfo environmentInfo)
    {
      switch (projectInfo.Type)
      {
        case ProjectType.WebApp:
          // deploy to all web server machines
          return new WebAppInputParams(environmentInfo.WebServerMachineNames);

        case ProjectType.Db:
          return new DbInputParams();

        case ProjectType.NtService:
          return new NtServiceInputParams();

        case ProjectType.SchedulerApp:
          return new SsdtInputParams();

        case ProjectType.TerminalApp:
          return new SsdtInputParams();

        case ProjectType.WebService:
          return new SsdtInputParams();

        case ProjectType.Extension:
          return new ExtensionInputParams();

        default:
          throw new DeploymentTaskException(string.Format("Project type: {0} is not supported in environment deployment.", projectInfo.Type));
      }      
    }

    private ProjectConfigurationBuild GetLatestSuccessfulBuild(string projectName, string projectConfigurationName)
    {
      const int maxBuildCount = 10;
      List<ProjectConfigurationBuild> projectBuilds = GetProjectConfigurationBuilds(projectName, projectConfigurationName, maxBuildCount, ProjectConfigurationBuildFilter.Empty);

      return projectBuilds.Where(x => x.Status == BuildStatus.Success)
        .OrderByDescending(x => x.StartDate)
        .FirstOrDefault();             
    }

    public List<Proxy.Dto.ProjectInfo> GetProjectInfos(Proxy.Dto.ProjectFilter projectFilter)
    {
      if (projectFilter == null)
      {
        throw new ArgumentNullException("projectFilter");
      }

      IEnumerable<ProjectInfo> projectInfos =
        _projectInfoRepository.GetAll();

      if (!string.IsNullOrEmpty(projectFilter.Name))
      {
        projectInfos =
          projectInfos
            .Where(pi => !string.IsNullOrEmpty(pi.Name) && pi.Name.IndexOf(projectFilter.Name, StringComparison.CurrentCultureIgnoreCase) > -1);
      }

      return
        projectInfos
          .Select(DtoMapper.Map<ProjectInfo, Proxy.Dto.ProjectInfo>)
          .ToList();
    }

    public List<Proxy.Dto.EnvironmentInfo> GetEnvironmentInfos()
    {
      IEnumerable<EnvironmentInfo> environmentInfos =
        _environmentInfoRepository.GetAll();

      return
        environmentInfos
          .Select(DtoMapper.Map<EnvironmentInfo, Proxy.Dto.EnvironmentInfo>)
          .ToList();
    }   

    public List<string> GetWebMachineNames(string environmentName)
    {
      if (string.IsNullOrEmpty(environmentName))
      {
        throw new ArgumentException("Environment name can't be null or empty", "environmentName");
      }

      EnvironmentInfo environmentInfo = _environmentInfoRepository.FindByName(environmentName);

      if (environmentInfo == null)
      {
        throw new FaultException<EnvironmentNotFoundFault>(
          new EnvironmentNotFoundFault
          {
            EnvironmentName = environmentName
          });
      }

      return environmentInfo.WebServerMachineNames.ToList();
    }

    public List<ProjectConfiguration> GetProjectConfigurations(string projectName, Proxy.Dto.ProjectConfigurationFilter projectConfigurationFilter)
    {
      if (string.IsNullOrEmpty(projectName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "projectName");
      }

      if (projectConfigurationFilter == null)
      {
        throw new ArgumentNullException("projectConfigurationFilter");
      }

      ProjectInfo projectInfo =
        _projectInfoRepository.FindByName(projectName);

      Core.TeamCity.Models.Project project =
        projectInfo != null
          ? _teamCityClient.GetProjectByName(projectInfo.ArtifactsRepositoryName)
          : null;

      Core.TeamCity.Models.ProjectDetails projectDetails =
        project != null
          ? _teamCityClient.GetProjectDetails(project)
          : null;

      if (projectDetails == null)
      {
        throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = projectName });
      }

      if (projectDetails.ConfigurationsList == null || projectDetails.ConfigurationsList.Configurations == null)
      {
        return new List<ProjectConfiguration>();
      }

      IEnumerable<Core.TeamCity.Models.ProjectConfiguration> projectConfigurations =
        projectDetails.ConfigurationsList.Configurations;

      if (!string.IsNullOrEmpty(projectConfigurationFilter.Name))
      {
        projectConfigurations =
          projectConfigurations
            .Where(pc => !string.IsNullOrEmpty(pc.Name) && pc.Name.IndexOf(projectConfigurationFilter.Name, StringComparison.CurrentCultureIgnoreCase) > -1);
      }

      return projectConfigurations
        .Select(DtoMapper.Map<Core.TeamCity.Models.ProjectConfiguration, ProjectConfiguration>)
        .ToList();
    }

    public List<ProjectConfigurationBuild> GetProjectConfigurationBuilds(string projectName, string projectConfigurationName, int maxCount, Proxy.Dto.ProjectConfigurationBuildFilter projectConfigurationBuildFilter)
    {
      if (string.IsNullOrEmpty(projectName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "projectName");
      }

      if (string.IsNullOrEmpty(projectConfigurationName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "projectConfigurationName");
      }

      if (projectConfigurationBuildFilter == null)
      {
        throw new ArgumentNullException("projectConfigurationBuildFilter");
      }

      ProjectInfo projectInfo =
        _projectInfoRepository.FindByName(projectName);

      Core.TeamCity.Models.Project project =
        projectInfo != null
          ? _teamCityClient.GetProjectByName(projectInfo.ArtifactsRepositoryName)
          : null;

      Core.TeamCity.Models.ProjectDetails projectDetails =
        project != null
          ? _teamCityClient.GetProjectDetails(project)
          : null;

      if (projectDetails == null)
      {
        throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = projectName });
      }

      Core.TeamCity.Models.ProjectConfiguration projectConfiguration =
        (projectDetails.ConfigurationsList != null && projectDetails.ConfigurationsList.Configurations != null)
          ? projectDetails.ConfigurationsList.Configurations
              .SingleOrDefault(pc => pc.Name == projectConfigurationName)
          : null;

      Core.TeamCity.Models.ProjectConfigurationDetails projectConfigurationDetails =
        projectConfiguration != null
          ? _teamCityClient.GetProjectConfigurationDetails(projectConfiguration)
          : null;

      if (projectConfigurationDetails == null)
      {
        throw new FaultException<ProjectConfigurationNotFoundFault>(new ProjectConfigurationNotFoundFault { ProjectName = projectInfo.Name, ProjectConfigurationName = projectConfigurationName });
      }

      Core.TeamCity.Models.ProjectConfigurationBuildsList projectConfigurationBuildsList =
        _teamCityClient.GetProjectConfigurationBuilds(projectConfigurationDetails, 0, maxCount);

      if (projectConfigurationBuildsList.Builds == null)
      {
        return new List<ProjectConfigurationBuild>();
      }

      IEnumerable<Core.TeamCity.Models.ProjectConfigurationBuild> projectConfigurationBuilds =
        projectConfigurationBuildsList.Builds;

      if (!string.IsNullOrEmpty(projectConfigurationBuildFilter.Number))
      {
        projectConfigurationBuilds =
          projectConfigurationBuilds
            .Where(pcb => !string.IsNullOrEmpty(pcb.Number) && pcb.Number.IndexOf(projectConfigurationBuildFilter.Number, StringComparison.CurrentCultureIgnoreCase) > -1);
      }

      return projectConfigurationBuilds
        .Select(DtoMapper.Map<Core.TeamCity.Models.ProjectConfigurationBuild, ProjectConfigurationBuild>)
        .ToList();
    }

    public List<string> GetWebAppProjectTargetUrls(string projectName, string environmentName)
    {
      if (string.IsNullOrEmpty(projectName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "projectName");
      }

      if (string.IsNullOrEmpty(environmentName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "environmentName");
      }

      WebAppProjectInfo webAppProjectInfo =
        _projectInfoRepository.FindByName(projectName) as WebAppProjectInfo;

      if (webAppProjectInfo == null)
      {
        throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = projectName });
      }

      EnvironmentInfo environmentInfo =
        _environmentInfoRepository.FindByName(environmentName);

      if (environmentInfo == null)
      {
        throw new FaultException<EnvironmentNotFoundFault>(new EnvironmentNotFoundFault { EnvironmentName = environmentName });
      }

      List<string> targetUrls =
        webAppProjectInfo.GetTargetUrls(environmentInfo)
          .ToList();

      return targetUrls;
    }

    public List<string> GetProjectTargetFolders(string projectName, string environmentName)
    {
      if (string.IsNullOrEmpty(projectName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "projectName");
      }

      if (string.IsNullOrEmpty(environmentName))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "environmentName");
      }

      ProjectInfo projectInfo =
        _projectInfoRepository.FindByName(projectName);

      if (projectInfo == null)
      {
        throw new FaultException<ProjectNotFoundFault>(new ProjectNotFoundFault { ProjectName = projectName });
      }

      EnvironmentInfo environmentInfo =
        _environmentInfoRepository.FindByName(environmentName);

      if (environmentInfo == null)
      {
        throw new FaultException<EnvironmentNotFoundFault>(new EnvironmentNotFoundFault { EnvironmentName = environmentName });
      }

      List<string> targetFolders =
        projectInfo.GetTargetFolders(ObjectFactory.Instance, environmentInfo)
          .ToList();

      return targetFolders;
    }

    public List<Proxy.Dto.DeploymentRequest> GetDeploymentRequests(int startIndex, int maxCount)
    {
      return
        _deploymentRequestRepository.GetDeploymentRequests(startIndex, maxCount)
          .Select(DtoMapper.Map<DeploymentRequest, Proxy.Dto.DeploymentRequest>)
          .ToList();
    }

    public List<Proxy.Dto.DiagnosticMessage> GetDiagnosticMessages(Guid uniqueClientId, long lastSeenMaxMessageId, Proxy.Dto.DiagnosticMessageType minMessageType)
    {
      if (uniqueClientId == Guid.Empty)
      {
        throw new ArgumentException("Argument can't be Guid.Empty.", "uniqueClientId");
      }

      return
        _diagnosticMessagesLogger.GetMessages(uniqueClientId, lastSeenMaxMessageId)
          .Select(DtoMapper.Map<DiagnosticMessage, Proxy.Dto.DiagnosticMessage>)
          .Where(dm => dm.Type >= minMessageType)
          .ToList();
    }

    public Proxy.Dto.Metadata.ProjectMetadata GetProjectMetadata(string projectName, string environmentName)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");
      Guard.NotNullNorEmpty(environmentName, "environmentName");

      try
      {
        ProjectMetadata projectMetadata =
          _projectMetadataExplorer.GetProjectMetadata(projectName, environmentName);

        return
          new Proxy.Dto.Metadata.ProjectMetadata
          {
            ProjectName = projectMetadata.ProjectName,
            EnvironmentName = projectMetadata.EnvironmentName,
            ProjectVersions =
              projectMetadata.ProjectVersions
                .Select(
                  pv =>
                  new MachineSpecificProjectVersion
                  {
                    MachineName = pv.MachineName,
                    ProjectVersion = pv.ProjectVersion,
                  }).ToList(),
          };
      }
      catch (Exception exc)
      {
        _log.ErrorIfEnabled(() => "Unhandled exception.", exc);
        
        throw;
      }
    }

    public void SetCollectedCredentialsForAsynchronousWebCredentialsCollector(Guid deploymentId, string password)
    {
      Guard.NotEmpty(deploymentId, "deploymentId");
      Guard.NotNullNorEmpty(password, "password");

      AsynchronousWebPasswordCollector.SetCollectedCredentials(deploymentId, password);
    }

    public string GetDefaultPackageDirPath(string environmentName, string projectName)
    {
      EnvironmentInfo environmentInfo = _environmentInfoRepository.FindByName(environmentName);

      if (environmentInfo == null)
      {
        throw new FaultException<EnvironmentNotFoundFault>(new EnvironmentNotFoundFault { EnvironmentName = environmentName });
      }

      if (string.IsNullOrEmpty(environmentInfo.ManualDeploymentPackageDirPath))
      {
        return null;
      }

      return _dirPathParamsResolver.ResolveParams(environmentInfo.ManualDeploymentPackageDirPath, projectName);           
    }

    public List<string> GetProjectsForEnvironmentDeploy(string environmentName)
    {
      EnvironmentDeployInfo environmentDeployInfo =
        _environmentDeployInfoRepository.FindByName(environmentName);

      if (environmentDeployInfo == null)
      {
        throw new FaultException<EnvironmentDeployConfigurationNotFoundFault>(new EnvironmentDeployConfigurationNotFoundFault { EnvironmentName = environmentName });
      }      

      return environmentDeployInfo.ProjectsToDeploy;
    }

    #endregion

    #region Private methods

    private void HandleDeploymentException(Exception exception, Guid uniqueClientId)
    {
      const string errorMessage = "Unhandled exception.";

      _diagnosticMessagesLogger
        .LogMessage(
          uniqueClientId,
          DiagnosticMessageType.Error,
          string.Format("{0}{1}", errorMessage, (exception != null ? Environment.NewLine + exception : " (no exception info)")));

      _log.ErrorIfEnabled(() => errorMessage, exception);
    }

    private void HandleProjectDeploymentException(Exception exception, Guid uniqueClientId, string projectName)
    {
      string errorMessage = string.Format("Unhandled exception while deploying project: {0}.", projectName);

      _diagnosticMessagesLogger
        .LogMessage(
          uniqueClientId,
          DiagnosticMessageType.Error,
          string.Format("{0}{1}", errorMessage, (exception != null ? Environment.NewLine + exception : " (no exception info)")));

      _log.ErrorIfEnabled(() => errorMessage, exception);
    }

    private void DoDeploy(Guid uniqueClientId, string requesterIdentity, Core.Domain.DeploymentInfo deploymentInfo, ProjectInfo projectInfo)
    {
      DeploymentTask deploymentTask = projectInfo.CreateDeploymentTask(ObjectFactory.Instance);      

      StartTask(deploymentTask, uniqueClientId, requesterIdentity, deploymentInfo);
    }

    private void DoCreatePackage(Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfoDto, ProjectInfo projectInfo, string packageDirPath)
    {
      var deploymentTask =
        new CreateManualDeploymentPackageDeploymentTask(
          ObjectFactory.Instance.CreateProjectInfoRepository(),
          ObjectFactory.Instance.CreateEnvironmentInfoRepository(),
          ObjectFactory.Instance.CreateArtifactsRepository(),
          ObjectFactory.Instance.CreateDirectoryAdapter(),
          ObjectFactory.Instance.CreateFileAdapter(),
          ObjectFactory.Instance.CreateZipFileAdapter(),
          packageDirPath);

      Core.Domain.DeploymentInfo deploymentInfo =
        DtoMapper.ConvertDeploymentInfo(deploymentInfoDto, projectInfo);

      StartTask(deploymentTask, uniqueClientId, requesterIdentity, deploymentInfo);
    }

    private void StartTask(DeploymentTask deploymentTask, Guid uniqueClientId, string requesterIdentity, Core.Domain.DeploymentInfo deploymentInfo)
    {
      var deploymentContext =
        new DeploymentContext(requesterIdentity);

      EventHandler<DiagnosticMessageEventArgs> deploymentPipelineDiagnosticMessageAction =
        (eventSender, tmpArgs) => LogMessage(uniqueClientId, tmpArgs.MessageType, tmpArgs.Message);

      try
      {
        _deploymentPipeline.DiagnosticMessagePosted += deploymentPipelineDiagnosticMessageAction;

        _deploymentPipeline.StartDeployment(deploymentInfo, deploymentTask, deploymentContext);
      }
      finally
      {
        _deploymentPipeline.DiagnosticMessagePosted -= deploymentPipelineDiagnosticMessageAction;
      }
    }

    private void LogMessage(Guid uniqueClientId, DiagnosticMessageType messageType, string message)
    {
      _log.DebugIfEnabled(() => string.Format("{0}: {1}", messageType, message));

      _diagnosticMessagesLogger.LogMessage(uniqueClientId, messageType, message);
    }

    #endregion
  }
}
