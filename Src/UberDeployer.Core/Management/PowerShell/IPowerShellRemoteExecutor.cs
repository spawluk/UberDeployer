using System.Management.Automation;

namespace UberDeployer.Core.Management.PowerShell
{
  public interface IPowerShellRemoteExecutor
  {
    PSObject Execute(string script);
  }
}