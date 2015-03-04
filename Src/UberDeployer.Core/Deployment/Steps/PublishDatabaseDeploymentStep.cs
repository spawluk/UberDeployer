using System.IO;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Db;

namespace UberDeployer.Core.Deployment.Steps
{
  public class PublishDatabaseDeploymentStep : DeploymentStep
  {
    private readonly DbProjectInfo _dbProjectInfo;
    private readonly DatabaseServer _databaseServer;
    private readonly string _artifactsDirPath;
    private readonly IMsSqlDatabasePublisher _databasePublisher;

    public PublishDatabaseDeploymentStep(DbProjectInfo dbProjectInfo, DatabaseServer databaseServer, string artifactsDirPath, IMsSqlDatabasePublisher databasePublisher)
    {
      Guard.NotNull(dbProjectInfo, "dbProjectInfo");
      Guard.NotNull(databaseServer, "databaseServer");
      Guard.NotNull(artifactsDirPath, "artifactsDirPath");
      Guard.NotNull(databasePublisher, "databasePublisher");

      _dbProjectInfo = dbProjectInfo;
      _databaseServer = databaseServer;
      _artifactsDirPath = artifactsDirPath;
      _databasePublisher = databasePublisher;
    }

    protected override void DoExecute()
    {
      string dacpacFilePath = Path.Combine(_artifactsDirPath, _dbProjectInfo.GetDacpacFileName());
      
      _databasePublisher.PublishFromDacpac(dacpacFilePath, _dbProjectInfo.DbName, _databaseServer.MachineName);
    }

    public override string Description
    {
      get { return string.Format("Publish database {0} on server {1}", _dbProjectInfo.DbName, _databaseServer.MachineName); }
    }
  }
}