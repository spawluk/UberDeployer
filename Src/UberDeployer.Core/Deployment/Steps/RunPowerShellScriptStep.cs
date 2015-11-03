using System;
using System.IO;
using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Management.PowerShell;

namespace UberDeployer.Core.Deployment.Steps
{
  public class RunPowerShellScriptStep : DeploymentStep
  {
    private readonly string _machineName;
    private readonly Lazy<string> _lazyScriptPath;
    private readonly string _scriptName;    

    public RunPowerShellScriptStep(string machineName, Lazy<string> lazyScriptPath, string scriptName)
    {
      _machineName = machineName;
      _lazyScriptPath = lazyScriptPath;
      _scriptName = scriptName;
    }

    protected override void DoExecute()
    {
      try
      {
        var powerShellRemoteExecutor = new PowerShellRemoteExecutor(_machineName, LogOutput, LogError);

        string script = Path.Combine(_lazyScriptPath.Value, _scriptName);

        powerShellRemoteExecutor.Execute(script);

        PostDiagnosticMessage(string.Format("PowerShell script executed successfully, script: {0}", _lazyScriptPath), DiagnosticMessageType.Info);
      }
      catch (Exception exc)
      {
        PostDiagnosticMessage(string.Format("PowerShell script execution failed, script: {0}", _lazyScriptPath), DiagnosticMessageType.Error);

        throw new DeploymentTaskException(string.Format("Error while executing PowerShell script: {0}", _lazyScriptPath), exc);
      }
    }

    private void LogError(string errorMessage)
    {
      PostDiagnosticMessage(errorMessage, DiagnosticMessageType.Error);
      Console.WriteLine(errorMessage);
    }

    private void LogOutput(string outputMessage)
    {
      PostDiagnosticMessage(outputMessage, DiagnosticMessageType.Trace);
      Console.WriteLine(outputMessage);
    }

    public override string Description
    {
      get { return string.Format("Run PowerShell script: {0} on server {1}", _lazyScriptPath, _machineName); }
    }
  }
}
