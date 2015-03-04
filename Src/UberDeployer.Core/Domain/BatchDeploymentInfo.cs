using System.Collections.Generic;

namespace UberDeployer.Core.Domain
{
  public class BatchDeploymentInfo
  {
    public BatchDeploymentInfo(IEnumerable<ProjectDeploymentInfo> deployments)
    {
      Deployments = deployments;
    }

    public IEnumerable<ProjectDeploymentInfo> Deployments { get; private set; }
  }
}
