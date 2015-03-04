using System;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Db
{
  public class CreateDatabaseOptions
  {
    public CreateDatabaseOptions(string databaseName, DbFileOptions dataFileOptions, DbFileOptions logFileOptions)
    {
      Guard.NotNullNorEmpty(databaseName, "databaseName");

      if (logFileOptions != null && dataFileOptions == null)
      {
        throw  new ArgumentException("Must specify dataFileOptions to set logFileOptions");
      }

      DatabaseName = databaseName;
      DataFileOptions = dataFileOptions;
      LogFileOptions = logFileOptions;
    }

    public CreateDatabaseOptions(string databaseName)
      : this(databaseName, null, null)
    {
      DatabaseName = databaseName;
    }

    public string DatabaseName { get; private set; }

    public DbFileOptions DataFileOptions { get; private set; }
    
    public DbFileOptions LogFileOptions { get; private set; }
  }
}