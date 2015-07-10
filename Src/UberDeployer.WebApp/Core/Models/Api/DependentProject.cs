namespace UberDeployer.WebApp.Core.Models.Api
{
  public class DependentProject
  {
    string ProjectName { get; set; }

    string BranchName { get; set; }

    string BuildNumber { get; set; }
  }
}