using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.Cmd;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlDatabasePublisher : IMsSqlDatabasePublisher
  {
    private const string SqlPackageExeName = "sqlpackage.exe";

    private const string ArgumentsTemplate = @"/Action:Publish /Quiet:False /SourceFile:{0} /TargetDatabaseName:{1} /TargetServerName:{2}";

    private const string VariableTemplate = @"/Variables:{0}='{1}'";

    private readonly ICmdExecutor _cmdExecutor;
    private readonly string _sqlPackageExePath;

    public MsSqlDatabasePublisher(ICmdExecutor cmdExecutor, string sqlPackageDirPath)
    {
      Guard.NotNull(cmdExecutor, "cmdExecutor");      

      _cmdExecutor = cmdExecutor;

      _sqlPackageExePath = string.IsNullOrEmpty(sqlPackageDirPath) 
        ? SqlPackageExeName 
        : Path.Combine(sqlPackageDirPath, SqlPackageExeName);
    }

    public void PublishFromDacpac(string dacpacFilePath, string databaseName, string databaseServer, Dictionary<string, string> variables)
    {      
      string arguments = string.Format(ArgumentsTemplate, dacpacFilePath, databaseName, databaseServer);

      if (variables != null && variables.Any())
      {
        arguments += " " + FormatVariables(variables);
      }      

      _cmdExecutor.Execute(_sqlPackageExePath, arguments);
    }

    private static string FormatVariables(Dictionary<string, string> variables)
    {
      IEnumerable<string> formattedVariables = variables.Select(x => string.Format(VariableTemplate, x.Key, x.Value));

      return string.Join(" ", formattedVariables);
    }
  }
}
