using System;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.PowerShell;

namespace UberDeployer.Core.Deployment.Steps
{
  public class RemoveRemoteDirectoryStep : DeploymentStep
  {
    private readonly string _machineName;
    private readonly Lazy<string> _directoryPathToRemove;

    private const string RemoveDirScriptTemplate = "Remove-Item \"{0}\" -Force -Recurse";

    public RemoveRemoteDirectoryStep(string machineName, Lazy<string> directoryPathToRemove)
    {
      Guard.NotNullNorEmpty(machineName, "machineName");
      Guard.NotNull(directoryPathToRemove, "directoryPathToRemove");

      _machineName = machineName;
      _directoryPathToRemove = directoryPathToRemove;
    }

    protected override void DoExecute()
    {
      var powerShellRemoteExecutor = new PowerShellRemoteExecutor(_machineName, OnOutput, OnError);

      string script = string.Format(RemoveDirScriptTemplate, _directoryPathToRemove.Value);

      powerShellRemoteExecutor.Execute(script);
    }

    public override string Description
    {
      get { return string.Format("Removes [{0}] directory on machine: [{1}]", _directoryPathToRemove, _machineName); }
    }

    private void OnOutput(string outputMessage)
    {
      PostDiagnosticMessage(outputMessage, DiagnosticMessageType.Trace);
    }

    private void OnError(string errorMessage)
    {
      PostDiagnosticMessage(errorMessage, DiagnosticMessageType.Error);
    }
  }
}
