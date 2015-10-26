using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json.Linq;

using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.TeamCity.ApiModels;

namespace UberDeployer.Core.TeamCity
{
  public class TeamCityRestClient : ITeamCityRestClient
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private const string _guestAuthenticationType = "guestAuth";

    private const string _httpAuthenticationType = "httpAuth";

    private const string _basePathTemplate = "app/rest";

    private readonly string _url;

    private readonly string _basePath;

    private readonly string _userName;

    private readonly string _password;

    private readonly bool _isGuestMode;

    public TeamCityRestClient(Uri teamCityUrl)
    {
      Guard.NotNull(teamCityUrl, "teamCityUrl");

      _url = teamCityUrl.ToString().TrimEnd('/');

      _isGuestMode = true;

      _basePath = string.Format("{0}/{1}", _guestAuthenticationType, _basePathTemplate);
    }

    public TeamCityRestClient(Uri teamCityUrl, string userName, string password)
    {
      Guard.NotNull(teamCityUrl, "teamCityUrl");
      Guard.NotNullNorEmpty(userName, "userName");
      Guard.NotNullNorEmpty(password, "password");

      _url = teamCityUrl.ToString().TrimEnd('/');

      _userName = userName;

      _password = password;

      _isGuestMode = false;

      _basePath = string.Format("{0}/{1}", _httpAuthenticationType, _basePathTemplate);
    }

    public IEnumerable<TeamCityProject> GetAllProjects()
    {
      var response = ExecuteRequest("projects");

      if (response == null)
      {
        return Enumerable.Empty<TeamCityProject>();
      }

      var projects = ParseResponse<List<TeamCityProject>>(response["project"]);

      return projects;
    }

    public TeamCityProject GetProject(string projectName)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");

      var response = ExecuteRequest("projects/" + projectName);

      var project = ParseResponse<TeamCityProject>(response);

      return project;
    }

    public IEnumerable<TeamCityBuildType> GetBuildTypes(string projectName)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");

      var response = ExecuteRequest(string.Format("projects/{0}/buildTypes", projectName));

      if (response == null)
      {
        return Enumerable.Empty<TeamCityBuildType>();
      }

      var buildTypes = ParseResponse<List<TeamCityBuildType>>(response["buildType"]);

      return buildTypes;
    }

    public IEnumerable<TeamCityBuildType> GetBuildTypesWithBranches(string projectName)
    {
      Guard.NotNullNorEmpty(projectName, "projectName");

      var response = ExecuteRequest(string.Format("projects/{0}/buildTypes", projectName));

      if (response == null)
      {
        return Enumerable.Empty<TeamCityBuildType>();
      }

      var buildTypes = ParseResponse<List<TeamCityBuildType>>(response["buildType"]);

      foreach (var buildType in buildTypes)
      {
        var branchesReponse = ExecuteRequest(string.Format("buildTypes/id:{0}/branches", buildType.Id));

        buildType.Branches = ParseResponse<List<TeamCityBuildTypeBranch>>(branchesReponse["branch"]);
      }

      return buildTypes;
    }

    public IEnumerable<TeamCityBuild> GetBuilds(string buildTypeId, string branchName, int start, int count, bool onlySuccessful)
    {
      Guard.NotNullNorEmpty(buildTypeId, "buildTypeId");

      string branchLocator = string.IsNullOrWhiteSpace(branchName) ? string.Empty : string.Format("locator=branch:(name:{0})&", branchName);

      string statusLocator = onlySuccessful ? "&status=SUCCESS" : string.Empty;

      var response =
        ExecuteRequest(
          string.Format(
            "buildTypes/id:{0}/builds?{1}start={2}&count={3}{4}",
            buildTypeId,
            branchLocator,
            start,
            count,
            statusLocator));

      if (response == null)
      {
        return Enumerable.Empty<TeamCityBuild>();
      }

      var builds = ParseResponse<List<TeamCityBuild>>(response["build"]);

      return builds;
    }

    public TeamCityBuild GetBuild(string buildId)
    {
      Guard.NotNullNorEmpty(buildId, "buildId");

      var response = ExecuteRequest(string.Format("builds/id:{0}", buildId));

      var build = ParseResponse<TeamCityBuild>(response);

      build.BuildTypeId = response["buildType"]["id"].ToString();

      return build;
    }

    public TeamCityBuild GetLastSuccessfulBuild(string buildTypeId)
    {
      Guard.NotNullNorEmpty(buildTypeId, "buildTypeId");

      var response = ExecuteRequest(string.Format("builds/buildType:(id:{0}),status:SUCCESS,pinned:true", buildTypeId));

      var build = ParseResponse<TeamCityBuild>(response);

      build.BuildTypeId = response["buildType"]["id"].ToString();

      return build;
    }

    public void DownloadArtifacts(string buildId, string destinationFilePath)
    {
      Guard.NotNullNorEmpty(buildId, "buildTypeId");
      Guard.NotNullNorEmpty(destinationFilePath, "destinationFilePath");

      string authenticationType = _isGuestMode ? _guestAuthenticationType : _httpAuthenticationType;
      string requestUrl = string.Format("{0}/{1}/downloadArtifacts.html?buildId={2}", _url, authenticationType, buildId);

      try
      {
        using (var webClient = CreateWebClient())
        {
          webClient.DownloadFile(requestUrl, destinationFilePath);
        }
      }
      catch (Exception e)
      {
        throw new InternalException(string.Format("Cannot download artifacts for build id [{0}]", buildId), e);
      }
    }

    private JObject ExecuteRequest(string requestPath)
    {
      string requestUrl = CreateRequestUrl(requestPath);

      string response;

      try
      {
        using (var webClient = CreateWebClient())
        {
          response = webClient.DownloadString(requestUrl);
        }
      }
      catch (Exception e)
      {
        _log.Error(string.Format("Cannot execute request [{0}]", requestPath), e);
        return null;
      }

      return JObject.Parse(response);
    }

    private static T ParseResponse<T>(JToken response)
      where T : class
    {
      T responseObject = response.ToObject<T>();

      if (responseObject == null)
      {
        throw new InternalException("Parsed object is null.");
      }

      return responseObject;
    }

    private string CreateRequestUrl(string requestPath)
    {
      return string.Format("{0}/{1}/{2}", _url, _basePath, requestPath);
    }

    private WebClient CreateWebClient()
    {
      var webClient =
        new Http10WebClient
        {
          Proxy = GlobalProxySelection.GetEmptyWebProxy(),
        };

      if (_isGuestMode == false)
      {
        webClient.Credentials = new NetworkCredential(_userName, _password);
      }

      webClient.Headers.Add("Accept", "application/json");

      return webClient;
    }
  }
}