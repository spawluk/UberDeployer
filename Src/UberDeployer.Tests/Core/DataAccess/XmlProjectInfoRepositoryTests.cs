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
      const string dbProjectName = "UberDeployer.SampleDb";
      ProjectInfo info = _projectInfoRepository.FindByName(dbProjectName);
      
      DbProjectInfo dbInfo = (DbProjectInfo)info;

      Assert.AreEqual(3,dbInfo.Users.Count);
      Assert.AreEqual("Username1", dbInfo.Users.First());
    }

    [Test]
    public void NtSettingsLoadDependendProjectsCorrectly()
    {
      const string ntServiceProjectName = "UberDeployer.SampleNtService";
      ProjectInfo info = _projectInfoRepository.FindByName(ntServiceProjectName);
      
      NtServiceProjectInfo ntServiceProjectInfo = (NtServiceProjectInfo)info;

      Assert.AreEqual(1, ntServiceProjectInfo.DependendProjects.Count);
      Assert.IsTrue(string.Equals("UberDeployer.SampleDependendNtService", ntServiceProjectInfo.DependendProjects.First().ProjectName));
    }
  }
}
