using System.Collections.Generic;

namespace UberDeployer.Core.Domain
{
  public class EnvironmentDeployInfo
  {
    public EnvironmentDeployInfo(string targetEnvironment, List<string> projectsToDeploy)
    {
      TargetEnvironment = targetEnvironment;
      ProjectsToDeploy = projectsToDeploy;
    }

    public string TargetEnvironment { get; private set; }

    public List<string> ProjectsToDeploy { get; private set; }
  }
}