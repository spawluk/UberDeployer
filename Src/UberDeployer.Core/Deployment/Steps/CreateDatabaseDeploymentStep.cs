using System.IO;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Deployment.Steps
{
  public class CreateDatabaseDeploymentStep : DeploymentStep
  {
    private readonly DbProjectInfo _projectInfo;
    private readonly DatabaseServer _databaseServer;
    private readonly IDbManagerFactory _dbManagerFactory;

    public CreateDatabaseDeploymentStep(DbProjectInfo projectInfo, DatabaseServer databaseServer, IDbManagerFactory dbManagerFactoryFactory)
    {
      Guard.NotNull(projectInfo, "projectInfo");
      Guard.NotNull(databaseServer, "databaseServer");
      Guard.NotNull(dbManagerFactoryFactory, "dbManagerFactory");

      _projectInfo = projectInfo;
      _databaseServer = databaseServer;
      _dbManagerFactory = dbManagerFactoryFactory;
    }

    protected override void DoExecute()
    {
      CreateDatabaseOptions databaseOptions = BuildCreateDatabaseOptions();

      IDbManager dbManager = _dbManagerFactory.CreateDbManager(_databaseServer.MachineName);

      dbManager.CreateDatabase(databaseOptions);
    }

    private CreateDatabaseOptions BuildCreateDatabaseOptions()
    {
      if (string.IsNullOrEmpty(_databaseServer.DataDirPath))
      {
        return new CreateDatabaseOptions(_projectInfo.DbName);
      }

      string dataFileName = string.Format("{0}.mdf", _projectInfo.DbName);
      string dataFilePath = Path.Combine(_databaseServer.DataDirPath, dataFileName);
      var dataFileOptions = new DbFileOptions(_projectInfo.DbName, dataFilePath);

      DbFileOptions logFileOptions = null;

      if (!string.IsNullOrEmpty(_databaseServer.LogDirPath))
      {
        string logName = string.Format("{0}_log", _projectInfo.DbName);
        string logFileName = string.Format("{0}_log.ldf", _projectInfo.DbName);
        string logFilePath = Path.Combine(_databaseServer.LogDirPath, logFileName);
        logFileOptions = new DbFileOptions(logName, logFilePath);
      }

      return new CreateDatabaseOptions(_projectInfo.DbName, dataFileOptions, logFileOptions);
    }
    public override string Description
    {
      get
      {
        return string.Format(
          "Create database {0} on server {1}", 
          _projectInfo.DbName, 
          _databaseServer.MachineName);
      }
    }
  }
}