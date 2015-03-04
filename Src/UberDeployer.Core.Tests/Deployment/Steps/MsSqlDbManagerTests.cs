using NUnit.Framework;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;

namespace UberDeployer.Core.Tests.Deployment.Steps
{
  [TestFixture]
  public class MsSqlDbManagerTests
  {
    private MsSqlDbManager _msSqlDbManager;

    [SetUp]
    public void SetUp()
    {
      const string dbServerName = @"localhost\mssqlserver2012";
      _msSqlDbManager = new MsSqlDbManager(dbServerName);
    }

    [Test]
    public void DropDatabase_test()
    {
      // arrange  
      const string databaseName = "bind";

      // act
      _msSqlDbManager.DropDatabase(databaseName);

      // assert
    }

    [Test]
    public void DatabaseExist()
    {
      // arrange
      const string databaseName = "Thor2";

      // act
      bool databaseExist = _msSqlDbManager.DatabaseExist(databaseName);

      // assert
    }

    [Test]
    public void CreateDatabase()
    {
      // arrange  
      var databaseOptions = new CreateDatabaseOptions(
        "TestDb3", 
        new DbFileOptions("TestDb3_dat", @"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER2012\MSSQL\DATA\TestDb3dat.mdf"), 
        new DbFileOptions("TestDb3_log", @"C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER2012\MSSQL\DATA\TestDb3log.mdf"));

      // act
      _msSqlDbManager.CreateDatabase(databaseOptions);

      // assert
    }
  }
}
