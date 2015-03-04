namespace UberDeployer.Core.Domain
{
  public interface IEnvironmentDeployInfoRepository
  {
    EnvironmentDeployInfo FindByName(string environmentName);
  }
}