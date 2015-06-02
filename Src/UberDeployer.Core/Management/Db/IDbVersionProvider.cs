using System.Collections.Generic;

namespace UberDeployer.Core.Management.Db
{
  public interface IDbVersionProvider
  {
    bool CheckIfDatabaseExists(string dbName, string sqlServerName);

    IEnumerable<DbVersionInfo> GetVersions(string dbName, string sqlServerName);
  }
}