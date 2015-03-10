using UberDeployer.Core.Deployment.Tasks;

namespace UberDeployer.Core.Domain
{
  public class ProjectDeploymentData
  {
    public ProjectDeploymentData(DeploymentInfo deploymentInfo, ProjectInfo projectInfo, DeploymentTask deploymentTask)
    {
      DeploymentTask = deploymentTask;
      DeploymentInfo = deploymentInfo;
      ProjectInfo = projectInfo;
    }

    public DeploymentInfo DeploymentInfo { get; private set; }

    public ProjectInfo ProjectInfo { get; private set; }

    public DeploymentTask DeploymentTask { get; private set; }
  }
}