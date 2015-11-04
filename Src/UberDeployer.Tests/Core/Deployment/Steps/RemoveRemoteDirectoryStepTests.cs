using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Tests.Core.Deployment.Steps
{
  [TestFixture]
  public class RemoveRemoteDirectoryStepTests
  {
    [Test]
    [Ignore]
    public void TEST_METHOD()
    {
      // arrange
      var removeRemoteDirectoryStep = new RemoveRemoteDirectoryStep("D_APP03", new Lazy<string>(() => @"E:\LOGS\bla bal bla"));
      
      // act
      removeRemoteDirectoryStep.PrepareAndExecute();

      // assert
    }
  }
}
