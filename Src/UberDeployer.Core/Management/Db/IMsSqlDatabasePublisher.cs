using System.Collections.Generic;

namespace UberDeployer.Core.Management.Db
{
  public interface IMsSqlDatabasePublisher
  {
    void PublishFromDacpac(string dacpacFilePath, string databaseName, string databaseServer, Dictionary<string, string> variables);
  }
}