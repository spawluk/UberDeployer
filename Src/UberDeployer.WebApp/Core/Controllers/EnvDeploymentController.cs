using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using UberDeployer.Agent.Proxy;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.WebApp.Core.Models.Api;
using UberDeployer.WebApp.Core.Models.EnvDeploy;
using UberDeployer.WebApp.Core.Services;
using UberDeployer.WebApp.Core.Utils;

namespace UberDeployer.WebApp.Core.Controllers
{
  public class EnvDeploymentController : UberDeployerWebAppController
  {
    private readonly IAgentService _agentService;

    private readonly ISessionService _sessionService;

    public EnvDeploymentController(IAgentService agentService, ISessionService sessionService)
    {
      Guard.NotNull(agentService, "agentService");
      Guard.NotNull(sessionService, "sessionService");

      _agentService = agentService;
      _sessionService = sessionService;
    }

    public EnvDeploymentController()
      :this(new AgentServiceClient(), new SessionService())
    {
    }

    [HttpGet]
    public ActionResult Index(string env = null)
    {
      var viewModel =
        new EnvDeployViewModel
        {
          InitialTargetEnvironment = env
        };

      return View(viewModel);
    }

    [HttpGet]
    public ActionResult GetProjectsForEnvironmentDeploy(string environmentName)
    {
      List<string> projects = _agentService.GetProjectsForEnvironmentDeploy(environmentName);

      var projectViewModels = new List<EnvDeployProjectViewModel>();

      if (projects != null && projects.Any())
      {
        projectViewModels = projects.Select(proj =>
          new EnvDeployProjectViewModel
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

    [HttpPost]
    public ActionResult DeployAll(string environmentName)
    {
      if (string.IsNullOrWhiteSpace(environmentName))
      {
        return BadRequest();
      }

      Guid uniqueClientId = _sessionService.UniqueClientId;
      string requesterIdentity = SecurityUtils.CurrentUsername;

      _agentService.DeployEnvironmentAsync(uniqueClientId, requesterIdentity, environmentName);

      return Json(new { Status = "OK" });
    }
  }
}