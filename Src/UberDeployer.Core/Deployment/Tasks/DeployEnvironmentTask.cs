using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Deployment.Tasks
{
  public class DeployEnvironmentTask : DeploymentTask
  {
    public DeployEnvironmentTask(
      IProjectInfoRepository projectInfoRepository, 
      IEnvironmentInfoRepository environmentInfoRepository) 
      : base(projectInfoRepository, environmentInfoRepository)
    {
    }
  }
}
