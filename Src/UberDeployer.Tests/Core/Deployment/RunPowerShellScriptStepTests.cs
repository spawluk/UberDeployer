using System;
using NUnit.Framework;
using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Tests.Core.Deployment
{
  [TestFixture]
  public class RunPowerShellScriptStepTests
  {
    private RunPowerShellScriptStep _sut;

    [Test]
    public void TEST_METHOD()
    {
      // arrange
      const string scriptPath = @"C:\TEMP";

      _sut = new RunPowerShellScriptStep(
        machineName: "D_APP03",
        lazyScriptPath: new Lazy<string>(() => scriptPath), 
        scriptName: "Install.ps1");

      // act
      _sut.PrepareAndExecute();

      // assert
      
    }
  }
}
