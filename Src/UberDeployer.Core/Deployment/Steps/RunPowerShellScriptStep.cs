using System;
using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Management.PowerShell;

namespace UberDeployer.Core.Deployment.Steps
{
  public class RunPowerShellScriptStep : DeploymentStep
  {
    private readonly string _machineName;
    private readonly string _scriptPath;
    private readonly IFileAdapter _fileAdapter;

    public RunPowerShellScriptStep(string machineName, string scriptPath, IFileAdapter fileAdapter)
    {
      _machineName = machineName;
      _scriptPath = scriptPath;
      _fileAdapter = fileAdapter;
    }

    protected override void DoExecute()
    {
      try
      {
        var powerShellRemoteExecutor = new PowerShellRemoteExecutor(_machineName, LogOutput, LogError);

        string script = _fileAdapter.ReadAllText(_scriptPath);

        bool executedSuccessfully = powerShellRemoteExecutor.Execute(script);

        if (executedSuccessfully)
        {
          PostDiagnosticMessage(string.Format("PowerShell script executed successfully, script: {0}", _scriptPath), DiagnosticMessageType.Info);
        }
        else
        {
          PostDiagnosticMessage(string.Format("PowerShell script execution failed, script: {0}", _scriptPath), DiagnosticMessageType.Error);
        }
      }
      catch (Exception exc)
      {
        throw new DeploymentTaskException(string.Format("Error while executing PowerShell script: {0}", _scriptPath), exc);
      }
    }

    private void LogError(string errorMessage)
    {
      PostDiagnosticMessage(errorMessage, DiagnosticMessageType.Error);
    }

    private void LogOutput(string outputMessage)
    {
      PostDiagnosticMessage(outputMessage, DiagnosticMessageType.Trace);
    }

    public override string Description
    {
      get { return string.Format("Run PowerShell script: {0} on server {1}", _scriptPath, _machineName); }
    }
  }
}
