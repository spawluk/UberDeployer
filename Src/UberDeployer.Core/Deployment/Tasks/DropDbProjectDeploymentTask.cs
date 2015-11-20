using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Deployment.Tasks
{
  /// <summary>
  /// Publishes new clear database from dacpac file. Old database is removed.
  /// </summary>
  public class DropDbProjectDeploymentTask : DeploymentTask
  {
    private readonly IDbManagerFactory _dbManagerFactory;

    public DropDbProjectDeploymentTask(
      IProjectInfoRepository projectInfoRepository,
      IEnvironmentInfoRepository environmentInfoRepository,
      IDbManagerFactory dbManagerFactory)
      : base(projectInfoRepository, environmentInfoRepository)
    {
      Guard.NotNull(dbManagerFactory, "dbManagerFactory");

      _dbManagerFactory = dbManagerFactory;
    }

    protected override void DoPrepare()
    {
      EnvironmentInfo environmentInfo = GetEnvironmentInfo();
      DbProjectInfo projectInfo = GetProjectInfo<DbProjectInfo>();

      DbProjectConfiguration dbProjectConfiguration =
        environmentInfo.GetDbProjectConfiguration(projectInfo);

      DatabaseServer databaseServer =
        environmentInfo.GetDatabaseServer(dbProjectConfiguration.DatabaseServerId);

      // create step for dropping database
      var dropDatabaseDeploymentStep =
        new DropDatabaseDeploymentStep(
          projectInfo,
          databaseServer,
          _dbManagerFactory);

      AddSubTask(dropDatabaseDeploymentStep);
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