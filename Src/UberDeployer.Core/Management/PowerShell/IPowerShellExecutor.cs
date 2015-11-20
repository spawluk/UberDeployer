using System.Management.Automation;

namespace UberDeployer.Core.Management.PowerShell
{
  public interface IPowerShellExecutor
  {
    PSObject Execute(string script);
  }
}