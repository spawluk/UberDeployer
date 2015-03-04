namespace UberDeployer.Core.Management.Cmd
{
  public interface ICmdExecutor
  {
    void Execute(string fileToExecute, string arguments);
  }
}