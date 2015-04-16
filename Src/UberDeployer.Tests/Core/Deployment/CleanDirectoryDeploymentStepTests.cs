using System;
using System.IO;
using NUnit.Framework;
using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Tests.Core.Deployment
{
  [TestFixture]
  public class CleanDirectoryDeploymentStepTests
  {
    private string _workingDir;

    [SetUp]
    public void SetUp()
    {
      _workingDir = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString());

      Directory.CreateDirectory(_workingDir);
    }

    [TearDown]
    public void Finish()
    {
      Directory.Delete(_workingDir, true);
    }

    [Test]
    public void Excluded_dirs_are_not_cleaned()
    {
      var step = new CleanDirectoryDeploymentStep(new DirectoryAdapter(), new FileAdapter(),
        new Lazy<string>(() => _workingDir), new[] {"stay_here"});

      Directory.CreateDirectory(_workingDir + "/stay_here");

      step.Prepare();
      step.Execute();

      Assert.IsTrue(Directory.Exists(_workingDir + "/stay_here"));
    }

    [Test]
    public void Excluding_dirs_is_case_insensitive()
    {
      var step = new CleanDirectoryDeploymentStep(new DirectoryAdapter(), new FileAdapter(),
        new Lazy<string>(() => _workingDir), new[] { "STAY_HERE" });

      Directory.CreateDirectory(_workingDir + "/stay_here");

      step.Prepare();
      step.Execute();

      Assert.IsTrue(Directory.Exists(_workingDir + "/stay_here"));
    }
  }
}