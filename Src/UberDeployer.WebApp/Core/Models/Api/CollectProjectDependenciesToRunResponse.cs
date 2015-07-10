using System;
using System.Collections.Generic;

namespace UberDeployer.WebApp.Core.Models.Api
{
  public class CollectProjectDependenciesToDeployResponse
  {
    public Guid? DeploymentId { get; set; }

    public List<DependentProject> DependenciesToDeploy { get; set; }
  }
}