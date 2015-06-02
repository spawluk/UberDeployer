using System;
using System.Collections.Generic;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Domain.Input;

namespace UberDeployer.Core.Domain
{
  public class DbProjectInfo : ProjectInfo
  {
    private readonly string _dacpacFile;

    public DbProjectInfo(string name, string artifactsRepositoryName, IEnumerable<string> allowedEnvironmentNames, string artifactsRepositoryDirName, bool artifactsAreNotEnvironmentSpecific, string dbName, string databaseServerId, bool isTransactional, string dacpacFile)
      : base(name, artifactsRepositoryName, allowedEnvironmentNames, artifactsRepositoryDirName, artifactsAreNotEnvironmentSpecific)
    {
      _dacpacFile = dacpacFile;
      DbName = dbName;
      DatabaseServerId = databaseServerId;
      IsTransactional = isTransactional;
    }

    public override ProjectType Type
    {
      get { return ProjectType.Db; }
    }

    public override InputParams CreateEmptyInputParams()
    {
      return new DbInputParams();
    }

    public override DeploymentTask CreateDeploymentTask(IObjectFactory objectFactory)
    {
      if (objectFactory == null)
      {
        throw new ArgumentNullException("objectFactory");
      }

      return
        new DeployDbProjectDeploymentTask(
          objectFactory.CreateProjectInfoRepository(),
          objectFactory.CreateEnvironmentInfoRepository(),
          objectFactory.CreateArtifactsRepository(),
          objectFactory.CreateDbScriptRunnerFactory(),
          objectFactory.CreateDbVersionProvider(),
          objectFactory.CreateFileAdapter(),
          objectFactory.CreateZipFileAdapter(),
          objectFactory.CreateScriptsToRunWebSelector(),
          objectFactory.CreateMsSqlDatabasePublisher());
    }

    public override IEnumerable<string> GetTargetFolders(IObjectFactory objectFactory, EnvironmentInfo environmentInfo)
    {
      throw new NotSupportedException();
    }

    public override string GetMainAssemblyFileName()
    {
      throw new NotSupportedException();
    }

    public string GetDacpacFileName()
    {
      if (!string.IsNullOrWhiteSpace(_dacpacFile))
      {
        return _dacpacFile;
      }

      // by default dacpac file has the same name as artifacts repository
      return string.Format("{0}.dacpac", ArtifactsRepositoryName);
    }

    public string DbName { get; private set; }

    public string DatabaseServerId { get; set; }

    public bool IsTransactional { get; set; }    
  }
}
