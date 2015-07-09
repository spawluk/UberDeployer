namespace UberDeployer.Core.Management.Db.DbManager
{
  public interface IDbManager
  {
    void DropDatabase(string databaseName);

    void CreateDatabase(CreateDatabaseOptions databaseOptions);

    bool DatabaseExist(string databaseName);

    bool UserExists(string databaseName, string username);

    void AddUser(string databaseName, string username);

    void AddUserRoles(string databaseName, string username, params string[] roles);

    void AddReadWriteRolesToUser(string databaseName, string username);

    bool CheckIfUserIsInRole(string databaseName, string username, string roleName);

    bool CheckIfUserIsInReadWriteRoles(string databaseName, string username);
  }
}