using System.Collections.Generic;
using System.Linq;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Management.Cmd;

namespace UberDeployer.Core.Management.Db
{
  public class MsSqlDatabasePublisher : IMsSqlDatabasePublisher
  {
    private const string SqlPackageExe = "sqlpackage.exe";

    private const string ArgumentsTemplate = @"/Action:Publish /Quiet:False /SourceFile:{0} /TargetDatabaseName:{1} /TargetServerName:{2}";

    private const string VariableTemplate = @"/Variables:{0}='{1}'";

    private readonly ICmdExecutor _cmdExecutor;
    
    public MsSqlDatabasePublisher(ICmdExecutor cmdExecutor)
    {
      Guard.NotNull(cmdExecutor, "cmdExecutor");

      _cmdExecutor = cmdExecutor;
    }

    public void PublishFromDacpac(string dacpacFilePath, string databaseName, string databaseServer, Dictionary<string, string> variables)
    {      
      string arguments = string.Format(ArgumentsTemplate, dacpacFilePath, databaseName, databaseServer);

      if (variables != null && variables.Any())
      {
        arguments += " " + FormatVariables(variables);
      }

      _cmdExecutor.Execute(SqlPackageExe, arguments);
    }

    private static string FormatVariables(Dictionary<string, string> variables)
    {
      IEnumerable<string> formattedVariables = variables.Select(x => string.Format(VariableTemplate, x.Key, x.Value));

      return string.Join(" ", formattedVariables);
    }
  }
}
