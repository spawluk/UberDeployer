using System;

namespace UberDeployer.WebApp.Core.Models.Api
{
  public class CollectProjectDependenciesToRunResponse
  {
    public Guid? DeploymentId { get; set; }

    public string[] Dependencies { get; set; }
  }
}