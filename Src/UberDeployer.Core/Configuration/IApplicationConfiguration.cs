namespace UberDeployer.Core.Configuration
{
  public interface IApplicationConfiguration
  {
    string TeamCityHostName { get; }

    int TeamCityPort { get; }

    string TeamCityUserName { get; }

    string TeamCityPassword { get; }

    string ScExePath { get; }

    string ConnectionString { get; }
    
    string WebAppInternalApiEndpointUrl { get; }
    
    int WebAsynchronousPasswordCollectorMaxWaitTimeInSeconds { get; }
    
    string ManualDeploymentPackageCurrentDateFormat { get; }

    string AgentServiceEnvironmentName { get; }
    
    string SqlPackageDirPath { get; }

    bool CheckIfAppPoolExists { get; }

    bool DeployDependentProjects { get; }
  }
}
