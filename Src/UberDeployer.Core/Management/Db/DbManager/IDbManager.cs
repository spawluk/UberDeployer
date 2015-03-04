namespace UberDeployer.Core.Management.Db.DbManager
{
  public interface IDbManager
  {
    void DropDatabase(string databaseName);

    void CreateDatabase(CreateDatabaseOptions databaseOptions);

    bool DatabaseExist(string databaseName);
  }
}