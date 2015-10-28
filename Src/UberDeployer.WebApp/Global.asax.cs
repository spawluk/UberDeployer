using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using UberDeployer.Common;
using log4net;
using log4net.Config;

using UberDeployer.WebApp.Core.Infrastructure;

namespace UberDeployer.WebApp
{
  public class MvcApplication : HttpApplication
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
        "Default",
        "{controller}/{action}/{id}",
        new { controller = "Deployment", action = "Index", id = UrlParameter.Optional });
    }

    protected void Application_Start()
    {
      GlobalContext.Properties["applicationName"] = "UberDeployer.WebApp";
      XmlConfigurator.Configure();

      ViewEngines.Engines.Add(new UberDeployerViewEngine());

      AreaRegistration.RegisterAllAreas();

      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);
      
      _log.InfoIfEnabled(() => "Application has started.");
    }

    protected void Application_Error()
    {
      Exception exception = Server.GetLastError();
      HttpException httpException = exception as HttpException;

      if (httpException != null)
      {
        if (httpException.GetHttpCode() == 404)
        {
          return;
        }
      }

      _log.ErrorIfEnabled(() => "Unhandled exception.", exception);
    }
  }
}
