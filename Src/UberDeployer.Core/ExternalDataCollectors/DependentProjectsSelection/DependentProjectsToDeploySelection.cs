using System.Collections.Generic;

namespace UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection
{
  public class DependentProjectsToDeploySelection
  {
    public DependentProjectsToDeploySelection()
    {
      SelectedProjects = new List<DependentProject>();
    }

    public List<DependentProject> SelectedProjects { get; set; }
  }
}