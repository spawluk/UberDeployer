using System.Collections.Generic;

namespace UberDeployer.Core.Domain
{
  public class EnvironmentDeployInfo
  {
    public EnvironmentDeployInfo(string targetEnvironment, string buildConfigurationName, List<string> projectsToDeploy)
    {
      BuildConfigurationName = buildConfigurationName;
      TargetEnvironment = targetEnvironment;
      ProjectsToDeploy = projectsToDeploy;
    }

    public string TargetEnvironment { get; private set; }

    public string BuildConfigurationName { get; private set; }

    public List<string> ProjectsToDeploy { get; private set; }
  }
}