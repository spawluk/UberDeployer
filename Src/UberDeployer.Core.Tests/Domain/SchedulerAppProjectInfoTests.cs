﻿using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.ScheduledTasks;

namespace UberDeployer.Core.Tests.Domain
{
  [TestFixture]
  public class SchedulerAppProjectInfoTests
  {
    private const int _ExecutionTimeLimitInMinutes = 1;
    private const string _Name = "name";
    private const string _ArtifactsRepositoryName = "repoName";
    private const string _ArtifactsRepositoryDirName = "repoDirName";
    private const string _SchedulerAppName = "appName";
    private const string _SchedulerAppDirName = "appDirName";
    private const string _SchedulerAppExeName = "appExeName";
    private const string _SchedulerAppUserId = "appUser";
    private const int _ScheduledHour = 1;
    private const int _ScheduledMinute = 1;

    private static readonly List<EnvironmentUser> _EnvironmentUsers =
      new List<EnvironmentUser>
        {
          new EnvironmentUser("Sample.User", "some_user@centrala.kaczmarski.pl"),
        };

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_Name_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              null,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ArtifactsRepositoryName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              null,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ScheduledHour_LessThan0()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              -1,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ScheduledHour_GreaterThan23()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              24,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ScheduledMinute_GreaterThan59()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              60,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ScheduledMinute_LessThan0()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              -1,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_ExecutionTime_LessThan0()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              -1);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_SchedulerAppName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              null,
              _SchedulerAppDirName,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_SchedulerAppDirName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              null,
              _SchedulerAppExeName,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_SchedulerAppProjectInfoTests_Throws_When_SchedulerAppExeName_null()
    {
      Assert.Throws<ArgumentException>(
        () =>
          {
            new SchedulerAppProjectInfo(
              _Name,
              _ArtifactsRepositoryName,
              _ArtifactsRepositoryDirName,
              _SchedulerAppName,
              _SchedulerAppDirName,
              null,
              _SchedulerAppUserId,
              _ScheduledHour,
              _ScheduledMinute,
              _ExecutionTimeLimitInMinutes);
          });
    }

    [Test]
    public void Test_CreateDeployemntTask_Throws_When_ObjectFactory_null()
    {
      var schedulerAppProjectInfo = new SchedulerAppProjectInfo(
        _Name,
        _ArtifactsRepositoryName,
        _ArtifactsRepositoryDirName,
        _SchedulerAppName,
        _SchedulerAppDirName,
        _SchedulerAppExeName,
        _SchedulerAppUserId,
        _ScheduledHour,
        _ScheduledMinute,
        _ExecutionTimeLimitInMinutes);

      Assert.Throws<ArgumentNullException>(
        () => schedulerAppProjectInfo.CreateDeploymentTask(
          null, "configName", "buildID", "targetEnvironmentName"));
    }

    [Test]
    public void Test_CreateDeployemntTask_RunsProperly_WhenAllIsWell()
    {
      var objectFactory = new Mock<IObjectFactory>(MockBehavior.Strict);
      var envInfoRepository = new Mock<IEnvironmentInfoRepository>(MockBehavior.Loose);
      var artifactsRepository = new Mock<IArtifactsRepository>(MockBehavior.Loose);
      var taskScheduler = new Mock<ITaskScheduler>(MockBehavior.Loose);
      var passwordCollector = new Mock<IPasswordCollector>(MockBehavior.Loose);

      var schedulerAppProjectInfo = new SchedulerAppProjectInfo(
        _Name,
        _ArtifactsRepositoryName,
        _ArtifactsRepositoryDirName,
        _SchedulerAppName,
        _SchedulerAppDirName,
        _SchedulerAppExeName,
        _SchedulerAppUserId,
        _ScheduledHour,
        _ScheduledMinute,
        _ExecutionTimeLimitInMinutes);

      objectFactory.Setup(o => o.CreateEnvironmentInfoRepository()).Returns(envInfoRepository.Object);
      objectFactory.Setup(o => o.CreateArtifactsRepository()).Returns(artifactsRepository.Object);
      objectFactory.Setup(o => o.CreateTaskScheduler()).Returns(taskScheduler.Object);
      objectFactory.Setup(o => o.CreatePasswordCollector()).Returns(passwordCollector.Object);

      schedulerAppProjectInfo.CreateDeploymentTask(
        objectFactory.Object, "configName", "buildID", "targetEnvironmentName");
    }

    [Test]
    public void Test_GetTargetFolde_RunsProperly_WhenAllIsWell()
    {
      string machine = Environment.MachineName;
      const string baseDirPath = "c:\\basedir";
      var envInfo = new EnvironmentInfo(
        "name", "templates", machine, new[] { "webmachine" }, "terminalmachine", "databasemachine", baseDirPath, "webbasedir", "c:\\scheduler", "terminal", _EnvironmentUsers);

      var schedulerAppProjectInfo = new SchedulerAppProjectInfo(
        _Name,
        _ArtifactsRepositoryName,
        _ArtifactsRepositoryDirName,
        _SchedulerAppName,
        _SchedulerAppDirName,
        _SchedulerAppExeName,
        _SchedulerAppUserId,
        _ScheduledHour,
        _ScheduledMinute,
        _ExecutionTimeLimitInMinutes);

      var result = schedulerAppProjectInfo.GetTargetFolders(envInfo);

      Assert.AreEqual("\\\\" + machine + "\\c$\\scheduler\\" + _SchedulerAppDirName, result);
    }

    [Test]
    public void Test_GetTargetFolde_Throws_EnvInfo_null()
    {
      var schedulerAppProjectInfo = new SchedulerAppProjectInfo(
        _Name,
        _ArtifactsRepositoryName,
        _ArtifactsRepositoryDirName,
        _SchedulerAppName,
        _SchedulerAppDirName,
        _SchedulerAppExeName,
        _SchedulerAppUserId,
        _ScheduledHour,
        _ScheduledMinute,
        _ExecutionTimeLimitInMinutes);

      Assert.Throws<ArgumentNullException>(() => schedulerAppProjectInfo.GetTargetFolders(null));
    }
  }
}
