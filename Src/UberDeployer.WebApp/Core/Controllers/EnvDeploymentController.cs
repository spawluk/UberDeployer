using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using UberDeployer.Agent.Proxy;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.WebApp.Core.Models.Api;
using UberDeployer.WebApp.Core.Models.Deployment;

namespace UberDeployer.WebApp.Core.Controllers
{
  public class EnvDeploymentController : UberDeployerWebAppController
  {
    private readonly IAgentService _agentService;

    public EnvDeploymentController(IAgentService agentService)
    {
      Guard.NotNull(agentService, "agentService");

      _agentService = agentService;
    }

    public EnvDeploymentController()
      :this(new AgentServiceClient())
    {
    }

    [HttpGet]
    public ActionResult Index(string env = null)
    {
      var viewModel =
        new IndexViewModel
        {
          InitialSelection =
            !string.IsNullOrEmpty(env)
              ? new InitialSelection
              {
                TargetEnvironmentName = env,
              }
              : null,
        };

      return View(viewModel);
    }

    [HttpGet]
    public ActionResult GetProjectsForEnvironmentDeploy(string environmentName)
    {
      List<string> projects = _agentService.GetProjectsForEnvironmentDeploy(environmentName);

      var projectViewModels = new List<ProjectViewModel>();

      if (projects != null && projects.Any())
      {
        projectViewModels = projects.Select(proj =>
          new ProjectViewModel
          {
            Name = proj,
          })
          .ToList();
      }

      return
        Json(
          new { projects = projectViewModels },
          JsonRequestBehavior.AllowGet);
    }
  }
}