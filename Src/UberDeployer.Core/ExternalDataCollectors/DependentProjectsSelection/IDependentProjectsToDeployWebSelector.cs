using System;
using System.Collections.Generic;

namespace UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection
{
  public interface IDependentProjectsToDeployWebSelector
  {
    DependentProjectsToDeploySelection GetSelectedProjectsToDeploy(Guid deploymentId, string userName, List<DependentProject> dependentProjects);    
  }
}