using UberDeployer.Core.Configuration;

namespace UberDeployer.Core.DataAccess.Xml
{
  public class ApplicationConfigurationXml : IApplicationConfiguration
  {
    public string TeamCityHostName { get; set; }

    public int TeamCityPort { get; set; }

    public string TeamCityUserName { get; set; }

    public string TeamCityPassword { get; set; }

    public string ScExePath { get; set; }

    public string ConnectionString { get; set; }

    public string WebAppInternalApiEndpointUrl { get; set; }

    public int WebAsynchronousPasswordCollectorMaxWaitTimeInSeconds { get; set; }

    public string ManualDeploymentPackageCurrentDateFormat { get; set; }

    public string AgentServiceEnvironmentName { get; set; }

    public string SqlPackageDirPath { get; set; }

    public bool CheckIfAppPoolExists { get; set; }

    public bool DeployDependentProjects { get; set; }
  }
}
