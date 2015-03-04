using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UberDeployer.Common.IO;
using UberDeployer.Core.DataAccess;
using UberDeployer.Core.DataAccess.Xml;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Cmd;
using UberDeployer.Core.Management.Db;
using UberDeployer.Core.Management.Db.DbManager;
using UberDeployer.Core.TeamCity;

namespace UberDeployer.Core.Tests.Deployment.Tasks
{
  [TestFixture]
  public class PublishDbProjectDeploymentTaskTests
  {
    private PublishDbProjectDeploymentTask _publishDbProjectDeploymentTask;

    [SetUp]
    public void SetUp()
    {
      const string projectFilePath = @"..\..\Data\ProjectInfos.xml";
      const string environmentFilePath = @"..\..\Data\EnvironmentInfo_Local.xml";

      IProjectInfoRepository projectInfoRepository = new XmlProjectInfoRepository(projectFilePath);
      IMsSqlDatabasePublisher databasePublisher = new MsSqlDatabasePublisher(new CmdExecutor());
      IEnvironmentInfoRepository environmentInfoRepository = new XmlEnvironmentInfoRepository(environmentFilePath);
      IFileAdapter fileAdapter = new FileAdapter();
      IArtifactsRepository artifactsRepository = new TeamCityArtifactsRepository(new TeamCityClient("teamcity", 90, "guest", "guest"));
      IDbManagerFactory dbManagerFactory = new MsSqlDbManagerFactory();
      IZipFileAdapter zipFileAdapter = new ZipFileAdapter();

      _publishDbProjectDeploymentTask = new PublishDbProjectDeploymentTask(
        projectInfoRepository, 
        environmentInfoRepository, 
        artifactsRepository, 
        fileAdapter, 
        zipFileAdapter, 
        dbManagerFactory, 
        databasePublisher);
    }

    [Test]
    public void PublishDbProject()
    {
      // arrange
      DeploymentInfo deploymentInfo = new DeploymentInfo(Guid.NewGuid(), false, "");
      _publishDbProjectDeploymentTask.Initialize(deploymentInfo);

      // act
      _publishDbProjectDeploymentTask.PrepareAndExecute();

      // assert
    }
  }
}
