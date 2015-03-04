namespace UberDeployer.Core.Management.Db.DbManager
{
  public class MsSqlDbManagerFactory : IDbManagerFactory
  {
    public IDbManager CreateDbManager(string databaseServer)
    {
      return new MsSqlDbManager(databaseServer);
    }
  }
}
