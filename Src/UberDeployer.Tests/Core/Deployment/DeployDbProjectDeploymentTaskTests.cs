using System;
using System.Collections.Generic;
using System.Linq;

using Moq;

using NUnit.Framework;

using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;
using UberDeployer.Tests.Core.Generators;

namespace UberDeployer.Tests.Core.Deployment
{
  [TestFixture]
  public class DeployDbProjectDeploymentTaskTests
  {
    private Mock<IProjectInfoRepository> _projectInfoRepositoryFake;
    private Mock<IEnvironmentInfoRepository> _environmentInfoRepositoryFake;
    private Mock<IArtifactsRepository> _artifactsRepositoryFake;
    private Mock<IDbScriptRunnerFactory> _dbScriptRunnerFactoryFake;
    private Mock<IDbVersionProvider> _dbVersionProviderFake;
    private Mock<IFileAdapter> _fileAdapterFake;
    private Mock<IZipFileAdapter> _zipFileAdapterFake;
    private Mock<IScriptsToRunWebSelector> _scriptsToRunWebSelectorFake;
    private Mock<IMsSqlDatabasePublisher> _databasePublisherFake;
    private DeployDbProjectDeploymentTask _deploymentTask;

    private Mock<IDbManager> _dbManagerFake;

    [SetUp]
    public void SetUp()
    {
      _projectInfoRepositoryFake = new Mock<IProjectInfoRepository>(MockBehavior.Loose);
      _environmentInfoRepositoryFake = new Mock<IEnvironmentInfoRepository>(MockBehavior.Loose);
      _artifactsRepositoryFake = new Mock<IArtifactsRepository>(MockBehavior.Loose);
      _dbScriptRunnerFactoryFake = new Mock<IDbScriptRunnerFactory>(MockBehavior.Loose);
      _dbVersionProviderFake = new Mock<IDbVersionProvider>(MockBehavior.Loose);
      _fileAdapterFake = new Mock<IFileAdapter>();
      _zipFileAdapterFake = new Mock<IZipFileAdapter>();
      _scriptsToRunWebSelectorFake = new Mock<IScriptsToRunWebSelector>();
      _databasePublisherFake = new Mock<IMsSqlDatabasePublisher>();
      _dbManagerFake = new Mock<IDbManager>();
      

      _projectInfoRepositoryFake
        .Setup(x => x.FindByName(It.IsAny<string>()))
        .Returns(ProjectInfoGenerator.GetDbProjectInfo());

      _environmentInfoRepositoryFake
        .Setup(x => x.FindByName(It.IsAny<string>()))
        .Returns(DeploymentDataGenerator.GetEnvironmentInfo);

      _dbScriptRunnerFactoryFake
        .Setup(x => x.CreateDbScriptRunner(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(new Mock<IDbScriptRunner>(MockBehavior.Loose).Object);

      _deploymentTask =
        new DeployDbProjectDeploymentTask(
          _projectInfoRepositoryFake.Object,
          _environmentInfoRepositoryFake.Object,
          _artifactsRepositoryFake.Object,
          _dbScriptRunnerFactoryFake.Object,
          _dbVersionProviderFake.Object,
          _fileAdapterFake.Object,
          _zipFileAdapterFake.Object,
          _scriptsToRunWebSelectorFake.Object,
          _databasePublisherFake.Object,
          _dbManagerFake.Object
          );

      _deploymentTask.Initialize(DeploymentInfoGenerator.GetDbDeploymentInfo());

      _dbVersionProviderFake.Setup(x => x.CheckIfDatabaseExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

      _databasePublisherFake.Setup(
        x =>
        x.PublishFromDacpac(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()));
    }

    [Test]
    public void Description_is_not_empty()
    {
      _deploymentTask.Prepare();

      Assert.IsNotNullOrEmpty(_deploymentTask.Description);
    }

    [Test]
    public void DoPrepare_calls_environment_info_repository()
    {
      // act
      _deploymentTask.Prepare();

      // assert
      _environmentInfoRepositoryFake.VerifyAll();
    }

    [Test]
    public void DoPrepare_fails_when_environment_info_repository_returns_null()
    {
      // act
      _environmentInfoRepositoryFake
        .Setup(x => x.FindByName(It.IsAny<string>()))
        .Returns((EnvironmentInfo)null);

      // assert
      Assert.Throws<DeploymentTaskException>(() => _deploymentTask.Prepare());
    }

    [Test]
    public void DoPrepare_calls_db_script_runner_factory_when_database_exists()
    {
      // act
      _deploymentTask.Prepare();

      // assert
      _dbScriptRunnerFactoryFake.VerifyAll();
    }

    [Test]
    public void DoPrepare_fails_when_db_script_runner_factory_returns_null_script_runner()
    {
      // act
      _dbScriptRunnerFactoryFake
        .Setup(x => x.CreateDbScriptRunner(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((IDbScriptRunner)null);

      // assert
      Assert.Throws<DeploymentTaskException>(() => _deploymentTask.Prepare());
    }

    [Test]
    public void DoPrepare_adds_deployment_step_when_database_exists()
    {
      // act
      _deploymentTask.Prepare();

      // assert
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(DownloadArtifactsDeploymentStep)));
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(ExtractArtifactsDeploymentStep)));
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(GatherDbScriptsToRunDeploymentStep)));
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(RunDbScriptsDeploymentStep)));
    }

    [Test]
    public void DoPrepare_adds_deployment_step_when_database_doesnt_exist()
    {
      // arrange
      _dbVersionProviderFake.Setup(x => x.CheckIfDatabaseExists(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

      // act
      _deploymentTask.Prepare();

      // assert
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(DownloadArtifactsDeploymentStep)));
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(ExtractArtifactsDeploymentStep)));
      Assert.IsTrue(_deploymentTask.SubTasks.Any(x => x.GetType() == typeof(PublishDatabaseDeploymentStep)));
    }


    [Test]
    public void DoPrepare_adds_steps_in_appropriate_order_when_database_exists()
    {
      // arrange
      Type[] stepsTypesOrder =
      {
        typeof(DownloadArtifactsDeploymentStep),
        typeof(ExtractArtifactsDeploymentStep),
        typeof(GatherDbScriptsToRunDeploymentStep),
        typeof(RunDbScriptsDeploymentStep)
      };

      // act
      _deploymentTask.Prepare();

      // assert
      int stepIndex = 0;

      foreach (var subtask in _deploymentTask.SubTasks)
      {
        Assert.IsTrue(subtask.GetType() == stepsTypesOrder[stepIndex]);
        stepIndex++;
      }
    }

    [Test]
    public void DoPrepare_adds_steps_in_appropriate_order_when_database_doesnt_exist()
    {
      // arrange
      _dbVersionProviderFake.Setup(x => x.CheckIfDatabaseExists(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

      Type[] stepsTypesOrder =
      {
        typeof(DownloadArtifactsDeploymentStep),
        typeof(ExtractArtifactsDeploymentStep),
        typeof(PublishDatabaseDeploymentStep),
      };

      // act
      _deploymentTask.Prepare();

      // assert
      int stepIndex = 0;

      foreach (var subtask in _deploymentTask.SubTasks)
      {
        Assert.IsTrue(subtask.GetType() == stepsTypesOrder[stepIndex]);
        stepIndex++;
      }
    }
  }
}