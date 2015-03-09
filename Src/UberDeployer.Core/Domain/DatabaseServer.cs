using System.Collections.Generic;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Domain
{
  public class DatabaseServer
  {
    public DatabaseServer(string id, string machineName, string dataDirPath = null, string logDirPath = null, Dictionary<string, string> sqlPackageVariables = null)
    {
      Guard.NotNullNorEmpty(id, "id");
      Guard.NotNullNorEmpty(machineName, "machineName");

      if (!string.IsNullOrEmpty(logDirPath))
      {
        Guard.NotNullNorEmpty(dataDirPath, "dataFilePath");
      }

      Id = id;
      MachineName = machineName;
      DataDirPath = dataDirPath;
      LogDirPath = logDirPath;
      SqlPackageVariables = sqlPackageVariables;
    }

    public string Id { get; private set; }

    public string MachineName { get; private set; }

    public string DataDirPath { get; private set; }

    public string LogDirPath { get; private set; }

    public Dictionary<string, string> SqlPackageVariables { get; private set; }
  }
}
