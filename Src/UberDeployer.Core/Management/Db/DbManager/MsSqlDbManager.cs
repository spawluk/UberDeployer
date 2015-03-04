using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Db.DbManager
{
  public class MsSqlDbManager : IDbManager
  {
    private const string _ConnectionStringPattern = "Server={0};Integrated Security=SSPI";
    private const string _DropDatabaseTemplate = "DROP DATABASE {0}";
    private const string _DbExistQueryTemplate = "SELECT TOP 1 1 FROM master.dbo.sysdatabases WHERE name='{0}'";

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
