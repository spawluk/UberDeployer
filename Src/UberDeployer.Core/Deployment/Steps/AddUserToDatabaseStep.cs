using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Deployment.Steps
{
  public class AddUserToDatabaseStep : DeploymentStep
  {
    private readonly IDbManager _dbManager;

    private string _username = string.Empty;
    private string _databaseName = string.Empty;

    public AddUserToDatabaseStep(IDbManager dbManager, string databaseName, string usermane)
    {
      _dbManager = dbManager;

      _username = usermane;
      _databaseName = databaseName;
    }

    protected override void DoExecute()
    {
      _dbManager.AddUser(_databaseName, _username);
    }

    public override string Description
    {
      get
      {
        return string.Format("Adding user '{0}' to database: {1}.", _username, _databaseName);
      }
    }
  }
}
