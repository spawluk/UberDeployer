using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.Cmd;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlSqlCmdScriptRunner : IDbScriptRunner
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static string SqlCmdExe = "sqlcmd.exe";

    private readonly string _databaseServer;
    private readonly string _databaseName;
    private readonly ICmdExecutor _cmdExecutor;

    private string _tempDirPath;
    private string _tmpScriptPath;

    public MsSqlSqlCmdScriptRunner(string databaseServer, string databaseName, ICmdExecutor cmdExecutor)
    {
      Guard.NotNullNorEmpty(databaseServer, "databaseServer");
      Guard.NotNullNorEmpty(databaseName, "databaseName");
      Guard.NotNull(cmdExecutor, "cmdExecutor");

      _databaseServer = databaseServer;
      _databaseName = databaseName;
      _cmdExecutor = cmdExecutor;
    }

    public void Execute(string scriptToExecute)
    {
      if (scriptToExecute == null)
      {
        throw new ArgumentNullException("scriptToExecute");
      }

      _tmpScriptPath = Path.Combine(GetTempDirPath(), Guid.NewGuid().ToString("N") + ".sql");
      File.WriteAllText(_tmpScriptPath, scriptToExecute, new UTF8Encoding(true));

      try
      {
        string arguments = string.Format("-S \"{0}\" -E -i \"{2}\" -b -v DatabaseName=\"{1}\"", _databaseServer, _databaseName, _tmpScriptPath);

        _cmdExecutor.Execute(SqlCmdExe, arguments);        

        _log.Debug("Applying script to database ended successfully.");
      }
      finally
      {
        File.Delete(_tmpScriptPath);
      }
    }   

    protected string GetTempDirPath()
    {
      if (string.IsNullOrEmpty(_tempDirPath))
      {
        string tempDirName = Guid.NewGuid().ToString("N");

        _tempDirPath = Path.Combine(Path.GetTempPath(), tempDirName);

        Directory.CreateDirectory(_tempDirPath);
      }

      return _tempDirPath;
    }
  }
}