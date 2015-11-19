using System;
using System.Management.Automation;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Management.PowerShell;

namespace UberDeployer.Core.Deployment.Steps
{
  public class CreateRemoteTempDirStep : DeploymentStep
  {
    private readonly string _machineName;

    private const string CreateTempDirScript = 
      "$TempDir = [System.Guid]::NewGuid().ToString();" + 
      "Set-Location ([System.IO.Path]::GetTempPath());" + 
      "New-Item -Type Directory -Name $TempDir;" + 
      "Set-Location $TempDir;" + 
      "(pwd).Path;";

    public CreateRemoteTempDirStep(string machineName)
    {
      Guard.NotNullNorEmpty(machineName, "machineName");

      _machineName = machineName;
    }

    protected override void DoExecute()
    {
      var powerShellRemoteExecutor = new PowerShellExecutor(_machineName, Environment.MachineName, OnOutput, OnError);

      PSObject psObject = powerShellRemoteExecutor.Execute(CreateTempDirScript);

      if (psObject == null || psObject.BaseObject == null || psObject.BaseObject is string == false)
      {
        throw new DeploymentTaskException(string.Format("Failed creating remote temp dir on machine: [{0}]", _machineName));
      }

      RemoteTempDirPath = psObject.BaseObject as string;
    }

    public override string Description
    {
      get { return string.Format("Creates temporary remote directory on machine: [{0}]", _machineName); }
    }

    private void OnOutput(string outputMessage)
    {
      PostDiagnosticMessage(outputMessage, DiagnosticMessageType.Trace);
    }

    private void OnError(string errorMessage)
    {
      PostDiagnosticMessage(errorMessage, DiagnosticMessageType.Error);
    }

    public string RemoteTempDirPath { get; private set; }
  }
}
