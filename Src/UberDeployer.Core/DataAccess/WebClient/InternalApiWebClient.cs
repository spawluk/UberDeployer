using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.ExternalDataCollectors;
using UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection;

namespace UberDeployer.Core.DataAccess.WebClient
{
  public class InternalApiWebClient : IInternalApiWebClient
  {
    private readonly string _internalApiEndpointUrl;

    public InternalApiWebClient(string internalApiEndpointUrl)
    {
      Guard.NotNullNorEmpty(internalApiEndpointUrl, "internalApiEndpointUrl");

      _internalApiEndpointUrl = internalApiEndpointUrl.TrimEnd('/'); ;
    }

    public void CollectScriptsToRun(Guid deploymentId, string[] sourceScriptsList)
    {
      using (var webClient = CreateWebClient())
      {
        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

        var data = new
        {
          DeploymentId = deploymentId,
          ScriptsToRun = sourceScriptsList
        };

        var jsonData = JsonConvert.SerializeObject(data);

        var result = webClient.UploadString(string.Format("{0}/CollectScriptsToRun", _internalApiEndpointUrl), jsonData);

        if (!string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase))
        {
          throw new InternalException("Something went wrong while requesting for database scripts to run.");
        }
      }
    }

    public void OnCollectScriptsToRunTimedOut(Guid deploymentId)
    {
      using (var webClient = CreateWebClient())
      {
        webClient.DownloadString(string.Format("{0}/OnCollectScriptsToRunTimedOut?deploymentId={1}", _internalApiEndpointUrl, deploymentId));
      }
    }

    public void CollectCredentials(Guid? deploymentId, string environmentName, string machineName, string userName)
    {
      using (var webClient = CreateWebClient())
      {
        string result =
          webClient.DownloadString(
            string.Format("{0}/CollectCredentials?deploymentId={1}&environmentName={2}&machineName={3}&username={4}", _internalApiEndpointUrl, deploymentId, environmentName, machineName, userName));

        if (!string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase))
        {
          throw new InternalException("Something went wrong while requesting for credentials collection.");
        }
      }
    }

    public void OnCollectCredentialsTimedOut(Guid deploymentId)
    {
      using (var webClient = CreateWebClient())
      {
        webClient.DownloadString(string.Format("{0}/OnCollectCredentialsTimedOut?deploymentId={1}", _internalApiEndpointUrl, deploymentId));
      }
    }

    public void CollectDependenciesToDeploy(Guid deploymentId, string userName, List<DependentProject> dependentProjects)
    {
      using (var webClient = CreateWebClient())
      {
        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

        var data = new
        {
          DeploymentId = deploymentId,
          UserName = userName,
          DependentProjects = dependentProjects
        };

        var jsonData = JsonConvert.SerializeObject(data);

        var result = webClient.UploadString(string.Format("{0}/CollectDependenciesToDeploy", _internalApiEndpointUrl), jsonData);

        if (!string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase))
        {
          throw new InternalException("Something went wrong while requesting for dependent projects to deploy.");
        }
      }
    }

    public void OnCollectDependenciesToDeployTimedOut(Guid deploymentId)
    {
      using (var webClient = CreateWebClient())
      {
        webClient.DownloadString(string.Format("{0}/OnCollectDependenciesToDeployTimedOut?deploymentId={1}", _internalApiEndpointUrl, deploymentId));
      }
    }

    private static System.Net.WebClient CreateWebClient()
    {
      return new System.Net.WebClient
      {
        UseDefaultCredentials = true
      };
    }
  }
}
