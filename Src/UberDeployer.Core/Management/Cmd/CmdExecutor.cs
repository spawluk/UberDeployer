using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using log4net;

namespace UberDeployer.Core.Management.Cmd
{
  public class CmdExecutor : ICmdExecutor
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public void Execute(string fileToExecute, string arguments)
    {
      var processStartInfo = new ProcessStartInfo
      {
        FileName = fileToExecute,
        CreateNoWindow = true,
        UseShellExecute = false,
        WindowStyle = ProcessWindowStyle.Hidden,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        Arguments = arguments
      };

      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      try
      {
        _log.DebugFormat("Start executing in cmd: {0} {1}", fileToExecute, arguments);

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

          StringBuilder sb = new StringBuilder();

          exeProcess.ErrorDataReceived += (sender, args) =>
          {
            if (string.IsNullOrEmpty(args.Data) == false)
            {
              _log.Error(args.Data);
              sb.AppendLine(args.Data);
            }
          };

          exeProcess.BeginErrorReadLine();
          exeProcess.BeginOutputReadLine();
          exeProcess.WaitForExit();

          if (exeProcess.ExitCode > 0)
          {
            _log.Error(string.Format("Error on executing command line. Error Code : [{0}], Message = [{1}].", exeProcess.ExitCode, sb));

            throw new CmdExecutionException(fileToExecute, arguments, exeProcess.ExitCode, sb.ToString());
          }

          _log.InfoFormat("Executed in cmd: {0} {1}", fileToExecute, arguments);
        }
      }
      catch (Exception exception)
      {
        _log.Error("Could not execute command.", exception);
      }
      finally
      {
        stopwatch.Stop();

        _log.DebugFormat("Executing file [{0}] took: {1} s.", fileToExecute, stopwatch.Elapsed.TotalSeconds);
      }
    }
  }
}
