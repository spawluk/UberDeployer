namespace UberDeployer.WebApp.Core.Models.Api
{
  public class DependentProject
  {
    public string ProjectName { get; set; }

    public string BranchName { get; set; }

    public string BuildNumber { get; set; }
  }
}