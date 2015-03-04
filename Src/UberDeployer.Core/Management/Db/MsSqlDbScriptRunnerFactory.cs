using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.Cmd;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlDbScriptRunnerFactory : IDbScriptRunnerFactory
  {
    private readonly ICmdExecutor _cmdExecutor;

    public MsSqlDbScriptRunnerFactory(ICmdExecutor cmdExecutor)
    {
      Guard.NotNull(cmdExecutor, "cmdExecutor");

      _cmdExecutor = cmdExecutor;
    }

    public IDbScriptRunner CreateDbScriptRunner(bool usesSqlCmdUpgradeScripts, string databaseServerName, string databaseName)
    {
      if (usesSqlCmdUpgradeScripts)
      {
        return new MsSqlSqlCmdScriptRunner(databaseServerName, databaseName, _cmdExecutor);
      }

      return new MsSqlDbScriptRunner(databaseServerName);
    }
  }
}