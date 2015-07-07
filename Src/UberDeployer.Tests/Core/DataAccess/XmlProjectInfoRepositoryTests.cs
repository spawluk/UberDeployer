﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using UberDeployer.Core.DataAccess.Xml;
using UberDeployer.Core.Domain;

namespace UberDeployer.Tests.Core.DataAccess
{
  [TestFixture]
  public class XmlProjectInfoRepositoryTests
  {
    private XmlProjectInfoRepository _projectInfoRepository;

    [SetUp]
    public void SetUp()
    {
      const string filename = "TestProjectInfo.xml";
      string path = Path.Combine("Core\\DataAccess", filename);
      _projectInfoRepository = new XmlProjectInfoRepository(path);
    }

    [Test]
    public void FileLoadSuccessful()
    {
      Assert.DoesNotThrow(() => _projectInfoRepository.GetAll());
    }

    [Test]
    public void DbSettingsLoadUsersCorrectly()
    {
      ProjectInfo info = _projectInfoRepository.FindByName("UberDeployer.SampleDb");
      
      DbProjectInfo dbInfo = (DbProjectInfo)info;

      Assert.AreEqual(3,dbInfo.Users.Count);
      Assert.AreEqual("Username1", dbInfo.Users.First());
    }

    [Test]
    public void NtSettingsLoadDependendProjectsCorrectly()
    {
      ProjectInfo info = _projectInfoRepository.FindByName("UberDeployer.SampleNtService");
      
      NtServiceProjectInfo ntServiceProjectInfo = (NtServiceProjectInfo)info;

      Assert.AreEqual(1, ntServiceProjectInfo.DependendProjects.Count);
      Assert.IsTrue(string.Equals("UberDeployer.SampleDependendNtService", ntServiceProjectInfo.DependendProjects.First().ProjectName));
    }

    [Test]
    public void FindProjectNameWithDependencies_throws_exception_when_configuration_not_found()
    {
      var output = _projectInfoRepository.FindProjectNameWithDependencies("xyz");
      Assert.AreEqual(0, output.Count);
    }

    [Test]
    public void FindProjectNameWithDependencies_find_district_dependencies()
    {
      var dependencyList = _projectInfoRepository.FindProjectNameWithDependencies("UberDeployer.SampleNtServiceWithDependences");
      Assert.AreEqual(4, dependencyList.Count);
      Assert.AreEqual("UberDeployer.SampleDb", dependencyList[0].Name);
      Assert.AreEqual("UberDeployer.SampleNtDependendService", dependencyList[1].Name);
      Assert.AreEqual("UberDeployer.SampleWebApp", dependencyList[2].Name);
      Assert.AreEqual("UberDeployer.SampleNtServiceWithDependences", dependencyList[3].Name);
    }
  }
}
