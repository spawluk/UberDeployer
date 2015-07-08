namespace UberDeployer.Core.Configuration
{
  public interface IApplicationConfigurationRepository
  {
    IApplicationConfiguration LoadConfiguration();
  }
}
