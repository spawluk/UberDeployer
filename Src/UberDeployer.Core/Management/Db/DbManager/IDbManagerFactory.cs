namespace UberDeployer.Core.Management.Db.DbManager
{
  public interface IDbManagerFactory
  {
    IDbManager CreateDbManager(string databaseServer);
  }
}