using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlSqlCmdScriptRunner : IDbScriptRunner
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static string SqlCmdExe = "sqlcmd.exe";

    private readonly string _databaseServer;
    private readonly string _databaseName;

    private string _tempDirPath;
    private string _tmpScriptPath;
    private static string _tmpErrorPath;

    public MsSqlSqlCmdScriptRunner(string databaseServer, string databaseName)
    {
      Guard.NotNullNorEmpty(databaseServer, "databaseServer");
      Guard.NotNullNorEmpty(databaseName, "databaseName");

      _databaseServer = databaseServer;
      _databaseName = databaseName;
    }

    public void Execute(string scriptToExecute)
    {
      if (scriptToExecute == null)
      {
        throw new ArgumentNullException("scriptToExecute");
      }

      _tmpScriptPath = Path.Combine(GetTempDirPath(), Guid.NewGuid().ToString("N") + ".sql");
      _tmpErrorPath = Path.Combine(GetTempDirPath(), Guid.NewGuid().ToString("N") + ".txt");

      File.WriteAllText(_tmpScriptPath, string.Format(":Error \"{0}\"\nGO\n", _tmpErrorPath), new UTF8Encoding(true));
      File.AppendAllText(_tmpScriptPath, scriptToExecute, new UTF8Encoding(true));

      try
      {
        string arguments = string.Format("-S \"{0}\" -E -i \"{2}\" -V15 -b -v DatabaseName=\"{1}\"", _databaseServer, _databaseName, _tmpScriptPath);

        Execute(SqlCmdExe, arguments);        

        _log.Debug("Applying script to database ended successfully.");
      }
      catch (Exception exception)
      {
        _log.Error(exception.Message);
        throw;
      }
      finally
      {
        File.Delete(_tmpScriptPath);
        File.Delete(_tmpErrorPath);
      }

    }

    private static void Execute(string fileToExecute, string arguments)
    {
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = fileToExecute;
      processStartInfo.CreateNoWindow = true;
      processStartInfo.UseShellExecute = false;
      processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      processStartInfo.RedirectStandardError = true;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.Arguments = arguments;

      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      try
      {
        _log.Info("Executing : " + fileToExecute + " " + arguments);
        using (Process exeProcess = Process.Start(processStartInfo))
        {
          exeProcess.EnableRaisingEvents = true;
          exeProcess.OutputDataReceived += (sender, args) =>
          {
            if (string.IsNullOrEmpty(args.Data) == false)
            {
              _log.Info(args.Data);
            }
          };

          exeProcess.BeginErrorReadLine();
          exeProcess.BeginOutputReadLine();
          exeProcess.WaitForExit();

          if (exeProcess.ExitCode > 0)
          {
            var sqlCmdError = File.Exists(_tmpErrorPath) ? File.ReadAllText(_tmpErrorPath) : string.Empty;

            _log.Error(string.Format("Error on executing command line. Error Code : [{0}], Message = [{1}]", exeProcess.ExitCode, sqlCmdError));
            throw new DbScriptRunnerException(string.Format("Error on executing command line. Error Code : [{0}], Message = [{1}]", exeProcess.ExitCode, sqlCmdError));
          }
        }
      }
      finally
      {
        stopwatch.Stop();

        _log.InfoFormat("Executing file [{0}] took: {1} s.", fileToExecute, stopwatch.Elapsed.TotalSeconds);
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