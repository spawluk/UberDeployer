using System;
using System.Collections.Generic;
using UberDeployer.Core.ExternalDataCollectors;
using UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection;

namespace UberDeployer.Core.DataAccess.WebClient
{
  public interface IInternalApiWebClient
  {
    void CollectScriptsToRun(Guid deploymentId, string[] sourceScriptsList);

    void OnCollectScriptsToRunTimedOut(Guid deploymentId);

    void CollectCredentials(Guid? deploymentId, string environmentName, string machineName, string userName);

    void OnCollectCredentialsTimedOut(Guid deploymentId);

    void CollectDependenciesToDeploy(Guid deploymentId, List<DependentProject> dependentProjects);

    void OnCollectDependenciesToDeployTimedOut(Guid deploymentId);
  }
}