using UberDeployer.Core.Domain;
using UberDeployer.Core.TeamCity.ApiModels;

namespace UberDeployer.Core.Deployment.Tasks.DependenciesDeployment
{
  public class ProjectDeployment
  {
    public ProjectInfo ProjectInfo { get; set; }

    public DeploymentInfo DeploymentInfo { get; set; }
    
    public TeamCityBuild TeamCityBuild { get; set; }
  }
}