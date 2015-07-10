using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Deployment.Steps
{
  public class AddRoleToUserStep : DeploymentStep
  {
    private readonly IDbManager _dbManager;

    private string _username = string.Empty;
    private string _databaseName = string.Empty;

    private string _dbRole;

    public AddRoleToUserStep(IDbManager dbManager, string databaseName, string username, string roleName)
    {
      _dbManager = dbManager;
      _databaseName = databaseName;
      _username = username;
      _dbRole = roleName;
    }

    protected override void DoExecute()
    {
      _dbManager.AddUserRole(_databaseName, _username, _dbRole);
    }

    public override string Description
    {
      get
      {
        return string.Format("Adding roles '{0}' to user: {1}.",_dbRole, _username);
      }
    }
  }
}
