namespace UberDeployer.Core.Configuration
{
  public interface IApplicationConfiguration
  {
    string TeamCityHostName { get; set; }

    int TeamCityPort { get; set; }

    string TeamCityUserName { get; set; }

    string TeamCityPassword { get; set; }

    string ScExePath { get; set; }

    string ConnectionString { get; set; }
    
    string WebAppInternalApiEndpointUrl { get; set; }
    
    int WebAsynchronousPasswordCollectorMaxWaitTimeInSeconds { get; set; }
    
    string ManualDeploymentPackageCurrentDateFormat { get; set; }

    string AgentServiceEnvironmentName { get; set; }
    
    string SqlPackageDirPath { get; set; }

    bool CheckIfAppPoolExists { get; set; }
  }
}
