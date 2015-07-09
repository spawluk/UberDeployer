using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Db.DbManager
{
  public class MsSqlDbManager : IDbManager
  {
    private const string _ConnectionStringPattern = "Server={0};Integrated Security=SSPI";
    private const string _DropDatabaseTemplate = "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {0}";
    private const string _DbExistQueryTemplate = "SELECT TOP 1 1 FROM master.dbo.sysdatabases WHERE name='{0}'";
    private const string _DbUserExistsOnDatabase = "USE {0}; SELECT TOP 1 1 FROM sys.database_principals WHERE name = N'{1}'";
    private const string _DbCreateUserOnDatabase = "USE {0}; CREATE USER [{1}] FOR LOGIN [{1}]";

    private const string _DbChekIfUserIsInRole = "USE {0}; SELECT TOP 1 1 FROM sys.sysmembers WHERE USER_NAME(groupuid) IN ('{1}') AND USER_NAME(memberuid) =  N'{2}';";

    private const string _DbAddRoleToUser = "USE {0}; EXEC sp_addrolemember N'{1}', N'{2}'";

    private readonly string[] _dbUserRoles = { "db_datareader", "db_datawriter" };

    private readonly string _databaseServer;

    public MsSqlDbManager(string databaseServer)
    {
      Guard.NotNull(databaseServer, "databaseServer");

      _databaseServer = databaseServer;
    }

    public void DropDatabase(string databaseName)
    {
      Guard.NotNullNorEmpty(databaseName, "databaseName");

      try
      {
        string dropDbCommand = BuildDropDatabaseCommand(databaseName);

        ExecuteNonQuery(dropDbCommand);        
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed dropping database {0} on server {1}.", databaseName, _databaseServer), exc);
      }
    }

    public void CreateDatabase(CreateDatabaseOptions databaseOptions)
    {
      Guard.NotNull(databaseOptions, "databaseOptions");

      try
      {
        string createDbCommand = BuildCreateDatabaseCommand(databaseOptions);

        ExecuteNonQuery(createDbCommand);
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed creating database {0} on server {1}.", databaseOptions.DatabaseName, _databaseServer), exc);
      }
    }

    public bool DatabaseExist(string databaseName)
    {
      try
      {
        string existDatabaseQuery = BuildExistDatabaseQuery(databaseName);

        object exist = ExecuteScalar(existDatabaseQuery);

        return exist != null;
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed checking if database {0} exist on server {1}.", databaseName, _databaseServer), exc);
      }
    }

    public bool UserExists(string databaseName, string username)
    {
      try
      {
        string userExistsQuery = BuildExistUserQuery(databaseName, username);

        object exists = ExecuteScalar(userExistsQuery);

        return exists != null;
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed checking if user {0} exist on database {1}.", username, databaseName), exc);
      }
    }

    public void AddUser(string databaseName, string username)
    {
      try
      {
        string addUserQuery = BuildAddUserQuery(databaseName, username);

        ExecuteNonQuery(addUserQuery);
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed creating user {0} on database {1}.", username, databaseName), exc);
      }
    }

    public void AddReadWriteRolesToUser(string databaseName, string username)
    {
      this.AddUserRoles(databaseName, username, this._dbUserRoles);
    }

    public void AddUserRoles(string databaseName, string username, params string[] roles)
    {
      foreach (string role in roles)
      {
        try
        {
          string addMembershipQuery = BuildAddMembershipQuery(databaseName, username, role);

          ExecuteNonQuery(addMembershipQuery);
        }
        catch (Exception exc)
        {
          throw new MsSqlDbManagementException(string.Format("Failed adding membership {0} to user {1}.", role, username), exc);
        }
      }
    }

    public bool CheckIfUserIsInRole(string databaseName, string username, string roleName)
    {
      try
      {
        string userIsInRoleQuery = BuildCheckUserIsInRoleQuery(databaseName, roleName, username);

        object exists = ExecuteScalar(userIsInRoleQuery);

        return exists != null;
      }
      catch (Exception exc)
      {
        throw new MsSqlDbManagementException(string.Format("Failed adding membership {0} to user {1}.", roleName, username), exc);
      }
    }

    public bool CheckIfUserIsInReadWriteRoles(string databaseName, string username)
    {
      return _dbUserRoles.All(x => CheckIfUserIsInRole(databaseName, username, x));
    }

    private void ExecuteNonQuery(string commandString)
    {
      using (var connection = new SqlConnection(GetConnectionString()))
      {
        Server server = GetServer(connection);        

        server.ConnectionContext.ExecuteNonQuery(commandString);
      }
    }

    private object ExecuteScalar(string queryString)
    {
      using (var connection = new SqlConnection(GetConnectionString()))
      {
        Server server = GetServer(connection);

        return server.ConnectionContext.ExecuteScalar(queryString);
      }
    }

    private static string BuildCreateDatabaseCommand(CreateDatabaseOptions databaseOptions)
    {
      Guard.NotNull(databaseOptions, "databaseOptions");
      Guard.NotNullNorEmpty(databaseOptions.DatabaseName, "databaseOptions.DatabaseName");

      var stringBuilder = new StringBuilder();

      stringBuilder.Append(string.Format("CREATE DATABASE {0}", databaseOptions.DatabaseName));

      if (databaseOptions.DataFileOptions != null)
      {
        stringBuilder.Append(string.Format(
          " ON ( NAME = {0}, FILENAME = '{1}')", 
          databaseOptions.DataFileOptions.Name, 
          databaseOptions.DataFileOptions.FileName));

        if (databaseOptions.LogFileOptions != null)
        {
          stringBuilder.Append(string.Format(
            " LOG ON ( NAME = {0}, FILENAME = '{1}')",
            databaseOptions.LogFileOptions.Name,
            databaseOptions.LogFileOptions.FileName));
        }
      }

      return stringBuilder.ToString();
    }

    private static string BuildDropDatabaseCommand(string databaseName)
    {
      return string.Format(_DropDatabaseTemplate, databaseName);
    }

    private static string BuildExistDatabaseQuery(string databaseName)
    {
      return string.Format(_DbExistQueryTemplate, databaseName);
    }

    private static string BuildExistUserQuery(string databaseName, string username)
    {
      return string.Format(_DbUserExistsOnDatabase, databaseName, username);
    }

    private string BuildAddUserQuery(string databaseName, string username)
    {
      return string.Format(_DbCreateUserOnDatabase, databaseName, username);
    }

    private string BuildAddMembershipQuery(string databaseName, string username, string role)
    {
      return string.Format(_DbAddRoleToUser, databaseName, role, username);
    }

    private string BuildCheckUserIsInRoleQuery(string database, string role, string username)
    {
      return string.Format(_DbChekIfUserIsInRole, database, role, username);
    }

    private static Server GetServer(SqlConnection connection)
    {
      var server = new Server(new ServerConnection(connection));
      
      var errors = new StringBuilder();

      server.ConnectionContext.ServerMessage += (o, eventArgs) => errors.AppendLine(eventArgs.ToString());
      server.ConnectionContext.InfoMessage += (o, eventArgs) => errors.AppendLine(eventArgs.ToString());

      return server;
    }   

    private string GetConnectionString()
    {
      return string.Format(_ConnectionStringPattern, _databaseServer);
    }
  }
}
