using System.Collections.Generic;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Domain.Input;

namespace UberDeployer.Core.Domain
{
  public class PowerShellScriptProjectInfo : ProjectInfo
  {
    public string _scriptPath;

    public PowerShellScriptProjectInfo(
      string name, 
      string artifactsRepositoryName, 
      IEnumerable<string> allowedEnvironmentNames, 
      string artifactsRepositoryDirName, 
      bool artifactsAreNotEnvironmentSpecific, 
      string scriptPath, 
      List<string> dependendProjectNames = null)
      : base(
        name, 
        artifactsRepositoryName, 
        allowedEnvironmentNames, 
        dependendProjectNames, 
        artifactsRepositoryDirName, 
        artifactsAreNotEnvironmentSpecific)
    {
      Guard.NotNullNorEmpty(scriptPath, "scriptPath");

      _scriptPath = scriptPath;
    }

    public override ProjectType Type
    {
      get { return ProjectType.PowerShellScript; }
    }

    public override InputParams CreateEmptyInputParams()
    {
      return new PowerShellInputParams();
    }

    public override DeploymentTask CreateDeploymentTask(IObjectFactory objectFactory)
    {
      return
        new DeployPowerShellScriptDeploymentTask(
          objectFactory.CreateProjectInfoRepository(),
          objectFactory.CreateEnvironmentInfoRepository(),
          objectFactory.CreateArtifactsRepository(),
          objectFactory.CreateFileAdapter(),
          objectFactory.CreateDirectoryAdapter(),
          objectFactory.CreateZipFileAdapter());
    }

    public override IEnumerable<string> GetTargetFolders(IObjectFactory objectFactory, EnvironmentInfo environmentInfo)
    {
      throw new System.NotImplementedException();
    }

    public override string GetMainAssemblyFileName()
    {
      throw new System.NotImplementedException();
    }

    public string MachineName { get; private set; }

    public string ScriptPath { get; private set; }
  }
}
