using System;

using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Core.Deployment
{
  public class ScriptsToRunSelectorForEnvironmentDeploy : IScriptsToRunSelector
  {
    public DbScriptsToRunSelection GetSelectedScriptsToRun(Guid deploymentId, string[] sourceScriptsList)
    {
      return new DbScriptsToRunSelection
      {
        DatabaseScriptToRunSelectionType = DatabaseScriptToRunSelectionType.LastVersion,
        SelectedScripts = sourceScriptsList
      };
    }
  }
}
