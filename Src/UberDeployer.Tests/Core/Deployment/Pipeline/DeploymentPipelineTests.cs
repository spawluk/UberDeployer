using System;

using Moq;

using NUnit.Framework;
using UberDeployer.Core;
using UberDeployer.Core.Configuration;
using UberDeployer.Core.Deployment.Pipeline;
using UberDeployer.Core.Domain;
using UberDeployer.Tests.Core.Generators;

namespace UberDeployer.Tests.Core.Deployment.Pipeline
{
  [TestFixture]
  public class DeploymentPipelineTests
  {
    private Mock<IApplicationConfiguration> _applicationConfigurationMock;
    private Mock<IObjectFactory> _objectFactoryMock;

    [SetUp]
    public void SetUp()
    {
      _applicationConfigurationMock = new Mock<IApplicationConfiguration>();
      _objectFactoryMock = new Mock<IObjectFactory>();
    }

    [Test]
    public void AddModule_WhenModuleIsNull_ThrowsArgumentNullException()
    {      
      var pipeline = new DeploymentPipeline(_applicationConfigurationMock.Object, _objectFactoryMock.Object);

      Assert.Throws<ArgumentNullException>(() => pipeline.AddModule(null));
    }

    [Test]
    public void StartDeployment_WhenDeploymentTaskIsNull_ThrowsArgumentNullException()
    {
      var projectInfoRepositoryFake = new Mock<IProjectInfoRepository>(MockBehavior.Loose);
      var environmentInfoRepositoryFake = new Mock<IEnvironmentInfoRepository>(MockBehavior.Loose);

      var pipeline = new DeploymentPipeline(_applicationConfigurationMock.Object, _objectFactoryMock.Object);

      var deploymentInfo = DeploymentInfoGenerator.GetNtServiceDeploymentInfo();

      Assert.Throws<ArgumentNullException>(() => pipeline.StartDeployment(null, new DummyDeploymentTask(projectInfoRepositoryFake.Object, environmentInfoRepositoryFake.Object), new DeploymentContext("requester"), false));
      Assert.Throws<ArgumentNullException>(() => pipeline.StartDeployment(deploymentInfo, null, new DeploymentContext("requester"), false));
      Assert.Throws<ArgumentNullException>(() => pipeline.StartDeployment(deploymentInfo, new DummyDeploymentTask(projectInfoRepositoryFake.Object, environmentInfoRepositoryFake.Object), null, false));
    }
  }
}
