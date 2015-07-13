using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Deployment.Tasks.DependenciesDeployment
{
  public class ProjectDeployment
  {
    public ProjectInfo ProjectInfo { get; set; }

    public DeploymentInfo DeploymentInfo { get; set; }
  }
}