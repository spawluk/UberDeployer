namespace UberDeployer.Core.Management.Db
{
  public class MsSqlDbScriptRunnerFactory : IDbScriptRunnerFactory
  {

    public IDbScriptRunner CreateDbScriptRunner(bool usesSqlCmdUpgradeScripts, string databaseServerName, string databaseName, string argumentsSqlCmd)
    {
      if (usesSqlCmdUpgradeScripts)
      {
        return new MsSqlSqlCmdScriptRunner(databaseServerName, databaseName, argumentsSqlCmd);
      }

      return new MsSqlDbScriptRunner(databaseServerName);
    }
  }
}