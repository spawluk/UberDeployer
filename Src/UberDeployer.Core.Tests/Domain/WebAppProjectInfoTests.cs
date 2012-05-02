﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.Iis;
using UberDeployer.Core.Management.MsDeploy;
using UberDeployer.Core.Management.ScheduledTasks;

namespace UberDeployer.Core.Tests.Domain
{
  // TODO IMM HI: formatting; code style
  [TestFixture]
  public class WebAppProjectInfoTests
  {
    private const string IisSiteName = "iisSiteName";
    private const string WebAppName = "WebAppName";
    private const string WebAppDirName = "WebAppDirName";
    private const string Name = "name";
    private const string ArtifactsRepositoryName = "repoName";
    private const string ArtifactsRepositoryDirName = "repoDirName";

    private static readonly List<EnvironmentUser> _EnvironmentUsers =
      new List<EnvironmentUser>
        {
          new EnvironmentUser("Sample.User", "some_user@centrala.kaczmarski.pl"),
        };

    private readonly IisAppPoolInfo AppPoolInfo = new IisAppPoolInfo(Name, IisAppPoolVersion.V4_0, IisAppPoolMode.Classic);

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_Name_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new WebAppProjectInfo(
            null,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);
        });
    }

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_ArtifactsRepositoryName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new WebAppProjectInfo(
            Name,
            null,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);
        });
    }

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_IisSiteName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            null,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);
        });
    }

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_WebAppName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            null,
            WebAppDirName,
            AppPoolInfo);
        });
    }

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_WebAppDirName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
        {
          new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            null,
            AppPoolInfo);
        });
    }

    [Test]
    public void Test_WebAppProjectInfoTests_Thows_When_AppPoolInfo_null()
    {
      Assert.Throws<ArgumentNullException>(
        () =>
        {
          new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            null);
        });
    }

    [Test]
    public void Test_CreateDeployemntTask_Thows_When_ObjectFactory_null()
    {
      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      Assert.Throws<ArgumentNullException>(
        () => projectInfo.CreateDeploymentTask(
          null, "configName", "buildID", "targetEnvironmentName"));
    }

    [Test]
    public void Test_CreateDeployemntTask_RunsProperly_WhenAllIsWell()
    {
      var objectFactory = new Mock<IObjectFactory>(MockBehavior.Strict);
      var envInfoRepository = new Mock<IEnvironmentInfoRepository>(MockBehavior.Strict);
      var artifactsRepository = new Mock<IArtifactsRepository>(MockBehavior.Strict);
      var taskScheduler = new Mock<ITaskScheduler>(MockBehavior.Strict);
      var imsDeploy = new Mock<IMsDeploy>(MockBehavior.Strict);
      var iisManager = new Mock<IIisManager>(MockBehavior.Strict);

      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      objectFactory.Setup(o => o.CreateEnvironmentInfoRepository()).Returns(envInfoRepository.Object);
      objectFactory.Setup(o => o.CreateArtifactsRepository()).Returns(artifactsRepository.Object); 
      objectFactory.Setup(o => o.CreateTaskScheduler()).Returns(taskScheduler.Object);
      objectFactory.Setup(o => o.CreateIMsDeploy()).Returns(imsDeploy.Object);
      objectFactory.Setup(o => o.CreateIIisManager()).Returns(iisManager.Object);
      
      projectInfo.CreateDeploymentTask(
          objectFactory.Object, "configName", "buildID", "targetEnvironmentName");
    }

    [Test]
    public void Test_GetTargetFolder_RunsProperly_WhenAllIsWell()
    {
      string machine = Environment.MachineName;

      var envInfo = new EnvironmentInfo(
        "name", "templates", machine, new[] { "webmachine" }, "terminalmachine", "databasemachine", "C:\\basedir", "C:\\basedir", "c:\\scheduler", "terminal", _EnvironmentUsers);

      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      Assert.IsNotNullOrEmpty(projectInfo.GetTargetFolders(envInfo).FirstOrDefault());
    }

    [Test]
    public void Test_GetTargetFolder_Throws_EnvInfo_null()
    {
      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      Assert.Throws<ArgumentNullException>(() => projectInfo.GetTargetFolders(null));
    }

    [Test]
    public void Test_GetTargetUrl_RunsProperly_WhenAllIsWell()
    {
      string machine = Environment.MachineName;
      const string baseDirPath = "c:\\basedir";
      var envInfo = new EnvironmentInfo(
        "name", "templates", machine, new[] { "webmachine" }, "terminalmachine", "databasemachine", baseDirPath, "webbasedir", "c:\\scheduler", "terminal", _EnvironmentUsers);

      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      var result = projectInfo.GetTargetUrls(envInfo);
      Assert.AreEqual("http://webmachine/" + WebAppName, result);
    }

    [Test]
    public void Test_GetTargetUrl_Throws_EnvInfo_null()
    {
      
      var projectInfo = new WebAppProjectInfo(
            Name,
            ArtifactsRepositoryName,
            ArtifactsRepositoryDirName,
            IisSiteName,
            WebAppName,
            WebAppDirName,
            AppPoolInfo);

      Assert.Throws<ArgumentNullException>(() => projectInfo.GetTargetUrls(null));
    }

    [Test]
    public void Test_WebServiceProjectInfoTests_RunsOK_WhenALLIZWELL()
    {
      new WebServiceProjectInfo(
                  Name,
                  ArtifactsRepositoryName,
                  ArtifactsRepositoryDirName,
                  IisSiteName,
                  WebAppName,
                  WebAppDirName,
                  AppPoolInfo);
    }

    [Test]
    public void Test_GetType_ReturnsType()
    {
      var info = new WebServiceProjectInfo(
                  Name,
                  ArtifactsRepositoryName,
                  ArtifactsRepositoryDirName,
                  IisSiteName,
                  WebAppName,
                  WebAppDirName,
                  AppPoolInfo);
      Assert.AreEqual("WebService", info.Type);
    }
  }
}
