using System;

namespace UberDeployer.Agent.Proxy.Dto.EnvDeployment
{
  public class ProjectToDeploy
  {
    public string ProjectName { get; set; }

    public Guid DeploymentId { get; set; }
  }
}
