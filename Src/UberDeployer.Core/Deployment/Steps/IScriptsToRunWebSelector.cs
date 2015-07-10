using System;

namespace UberDeployer.Core.Deployment.Steps
{
  public interface IScriptsToRunSelector
  {
    DbScriptsToRunSelection GetSelectedScriptsToRun(Guid deploymentId, string[] sourceScriptsList);
  }
}