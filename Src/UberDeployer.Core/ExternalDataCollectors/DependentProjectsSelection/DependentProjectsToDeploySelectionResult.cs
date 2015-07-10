namespace UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection
{
  internal class DependentProjectsToDeploySelectionResult
  {
    public bool Canceled { get; set; }

    public DependentProjectsToDeploySelection DependentProjectsToDeploySelection { get; set; }
  }
}