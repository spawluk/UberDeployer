using NUnit.Framework;
using UberDeployer.Core.Management.Cmd;
using UberDeployer.Core.Management.Db;

namespace UberDeployer.Core.Tests.Management.Db
{
  [TestFixture]
  public class MsSqlDatabasePublisherTests
  {
    private MsSqlDatabasePublisher _msSqlDatabasePublisher;

    [SetUp]
    public void SetUp()
    {
      _msSqlDatabasePublisher = new MsSqlDatabasePublisher(new CmdExecutor());
    }

    [Test]
    public void PublishFromDacpac()
    {
      // arrange  
      const string dacpacFilePath = @"C:\Users\m.rubin.KI-CENTRALA\Desktop\Constance.Database.dacpac";
      const string databaseName = "TestDb";
      const string databaseServer = @"localhost\mssqlserver2012";

      // act
      _msSqlDatabasePublisher.PublishFromDacpac(dacpacFilePath, databaseName, databaseServer);

      // assert
    }
  }
}
