using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Tests.Core.Deployment.Steps
{
  [TestFixture]
  public class CreateRemoteTempDirTests
  {
    [Test]
    public void TEST_METHOD()
    {
      // arrange
      var createRemoteTempDir = new CreateRemoteTempDir("D_APP03");

      // act
      createRemoteTempDir.Prepare();
      createRemoteTempDir.Execute();

      // assert
    }
  }
}
