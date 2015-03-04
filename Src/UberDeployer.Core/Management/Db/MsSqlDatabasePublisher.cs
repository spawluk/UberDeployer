using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.Cmd;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlDatabasePublisher : IMsSqlDatabasePublisher
  {
    const string SqlPackageExe = "sqlpackage.exe";

    const string ArgumentsTemplate = @"/Action:Publish /Quiet:False /SourceFile:{0} /TargetDatabaseName:{1} /TargetServerName:{2} /Variables:DacpacDirPath=''";

    private readonly ICmdExecutor _cmdExecutor;
    
    public MsSqlDatabasePublisher(ICmdExecutor cmdExecutor)
    {
      Guard.NotNull(cmdExecutor, "cmdExecutor");

      _cmdExecutor = cmdExecutor;
    }

    public void PublishFromDacpac(string dacpacFilePath, string databaseName, string databaseServer)
    {
      string arguments = string.Format(ArgumentsTemplate, dacpacFilePath, databaseName, databaseServer);

      _cmdExecutor.Execute(SqlPackageExe, arguments);
    }
  }
}
