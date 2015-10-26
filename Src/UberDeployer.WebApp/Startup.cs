using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(UberDeployer.WebApp.Startup))]
namespace UberDeployer.WebApp
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      GlobalHost.HubPipeline.RequireAuthentication();

      var config = new HubConfiguration
      {
        EnableDetailedErrors = true
      };

      app.MapHubs(config);
    }
  }
}