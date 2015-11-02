namespace UberDeployer.Core.Management.PowerShell
{
  public interface IPowerShellRemoteExecutor
  {
    bool Execute(string script);
  }
}