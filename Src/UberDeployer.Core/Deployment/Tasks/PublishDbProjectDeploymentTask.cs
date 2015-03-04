using System;
using UberDeployer.Common.IO;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Deployment.Tasks
{
  /// <summary>
  /// Publishes new clear database from dacpac file. Old database is removed.
  /// </summary>
  public class PublishDbProjectDeploymentTask : DeploymentTask
  {
    private readonly IArtifactsRepository _artifactsRepository;
    private readonly IFileAdapter _fileAdapter;
    private readonly IZipFileAdapter _zipFileAdapter;
    private readonly IDbManagerFactory _dbManagerFactory;
    private readonly IMsSqlDatabasePublisher _databasePublisher;

    public PublishDbProjectDeploymentTask(
      IProjectInfoRepository projectInfoRepository, 
      IEnvironmentInfoRepository environmentInfoRepository, 
      IArtifactsRepository artifactsRepository, 
      IFileAdapter fileAdapter, 
      IZipFileAdapter zipFileAdapter, 
      IDbManagerFactory dbManagerFactory, 
      IMsSqlDatabasePublisher databasePublisher) 
      : base(projectInfoRepository, environmentInfoRepository)
    {
      Guard.NotNull(artifactsRepository, "artifactsRepository");
      Guard.NotNull(fileAdapter, "fileAdapter");
      Guard.NotNull(zipFileAdapter, "zipFileAdapter");
      Guard.NotNull(dbManagerFactory, "dbManagerFactory");
      Guard.NotNull(databasePublisher, "databasePublisher");

      _artifactsRepository = artifactsRepository;
      _fileAdapter = fileAdapter;
      _zipFileAdapter = zipFileAdapter;
      _dbManagerFactory = dbManagerFactory;
      _databasePublisher = databasePublisher;
    }

    protected override void DoPrepare()
    {
      EnvironmentInfo environmentInfo = GetEnvironmentInfo();
      DbProjectInfo projectInfo = GetProjectInfo<DbProjectInfo>();

      DbProjectConfiguration dbProjectConfiguration =
        environmentInfo.GetDbProjectConfiguration(projectInfo);

      DatabaseServer databaseServer =
        environmentInfo.GetDatabaseServer(dbProjectConfiguration.DatabaseServerId);

      string artifactsDirPath = GetTempDirPath();

      // create a step for downloading the artifacts
      var downloadArtifactsDeploymentStep =
        new DownloadArtifactsDeploymentStep(
          projectInfo,
          DeploymentInfo,
          artifactsDirPath,
          _artifactsRepository);

      AddSubTask(downloadArtifactsDeploymentStep);

      // create a step for extracting the artifacts
      var extractArtifactsDeploymentStep =
        new ExtractArtifactsDeploymentStep(
          projectInfo,
          environmentInfo,
          DeploymentInfo,
          downloadArtifactsDeploymentStep.ArtifactsFilePath,
          GetTempDirPath(),
          _fileAdapter,
          _zipFileAdapter);

      AddSubTask(extractArtifactsDeploymentStep);

      // create step for dropping database
      var dropDatabaseDeploymentStep =
        new DropDatabaseDeploymentStep(
          projectInfo,
          databaseServer,
          _dbManagerFactory);

      AddSubTask(dropDatabaseDeploymentStep);

      // create step for creating database
      var createDatabaseDeploymentStep =
        new CreateDatabaseDeploymentStep(
          projectInfo,
          databaseServer,
          _dbManagerFactory);

      AddSubTask(createDatabaseDeploymentStep);

      // create step for deploying dacpac
      var publishDatabaseDeploymentStep =
        new PublishDatabaseDeploymentStep(
          projectInfo,
          databaseServer,
          artifactsDirPath,
          _databasePublisher);

      AddSubTask(publishDatabaseDeploymentStep);
    }

    public override string Description
    {
      get
      {
        return
          string.Format(
            "Deploy ssdt db project from dacpack '{0} ({1}:{2})' to '{3}'.",
            DeploymentInfo.ProjectName,
            DeploymentInfo.ProjectConfigurationName,
            DeploymentInfo.ProjectConfigurationBuildId,
            DeploymentInfo.TargetEnvironmentName);
      }
    }
  }
}
