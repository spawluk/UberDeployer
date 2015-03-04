using System;
using NUnit.Framework;
using UberDeployer.Agent.Service;
using UberDeployer.CommonConfiguration;

namespace UberDeployer.Agent.NTService.Tests
{
  [TestFixture]
  public class EvironmentDeploymentIntegrationTests
  {
    [SetUp]
    public void SetUp()
    {
      Bootstraper.Bootstrap();
    }

    [Test]
    public void DeployEnv()
    {
      // arrange  
      Guid uniqueClientId = Guid.NewGuid();
      const string requesterIdentity = "requester identity";
      const string targetEnvironment = "Local";

      var agentService = new AgentService();

      // act
      agentService.DeployEnvironmentAsync(uniqueClientId, requesterIdentity, targetEnvironment);

      // assert
    }
  }
}
