namespace UberDeployer.WebApp.Core.Models.EnvDeploy
{
  public class EnvDeployViewModel : BaseViewModel
  {
    public EnvDeployViewModel()
    {
      CurrentAppPage = AppPage.EnvDeployment;
    }

    public string InitialTargetEnvironment { get; set; }
  }
}