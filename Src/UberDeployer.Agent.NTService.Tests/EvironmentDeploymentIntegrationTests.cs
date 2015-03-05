using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UberDeployer.Agent.Service;
using UberDeployer.Agent.Service.Diagnostics;
using UberDeployer.CommonConfiguration;
using UberDeployer.Core.Deployment;

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
    [Ignore("Manual integration test")]
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
      IEnumerable<DiagnosticMessage> diagnosticMessages = InMemoryDiagnosticMessagesLogger.Instance.GetMessages(uniqueClientId, 0).ToList();
      Assert.IsFalse(diagnosticMessages.Any(), string.Join(",\n" , diagnosticMessages.Select(x => x.Message).ToList()));
    }
  }
}
