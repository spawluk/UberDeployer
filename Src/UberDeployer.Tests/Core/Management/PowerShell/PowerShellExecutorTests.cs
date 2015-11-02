using System;
using NUnit.Framework;
using UberDeployer.Core.Management.PowerShell;

namespace UberDeployer.Tests.Core.Management.PowerShell
{
  [TestFixture]
  public class PowerShellExecutorTests
  {
    private PowerShellRemoteExecutor _powerShellRemoteExecutor;

    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void TEST_METHOD()
    {
      // arrange
      _powerShellRemoteExecutor = new PowerShellRemoteExecutor(
        "D_APP03",
        Console.WriteLine,
        Console.WriteLine);

      // act
      _powerShellRemoteExecutor.Execute("fasd");

      // assert
    }


  }
}
