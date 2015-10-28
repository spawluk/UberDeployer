using System.Collections.Generic;
using System.Linq;

using UberDeployer.Core.TeamCity.ApiModels;

namespace UberDeployer.Core.TeamCity
{
  public class MockedTeamCityRestClient : ITeamCityRestClient
  {
    public IEnumerable<TeamCityProject> GetAllProjects()
    {
      return new List<TeamCityProject>()
      {
        GetProject("Project 1"),
        GetProject("Project 2"),
        GetProject("Project 3"),
        GetProject("Project 4"),
        GetProject("Project 5")
      };
    }

    public TeamCityProject GetProject(string projectName)
    {
      return new TeamCityProject
      {
        Id = "projectId",
        Name = projectName
      };
    }

    public IEnumerable<TeamCityBuildType> GetBuildTypes(string projectName)
    {
      return Enumerable.Empty<TeamCityBuildType>();
    }

    public IEnumerable<TeamCityBuildType> GetBuildTypesWithBranches(string projectName)
    {
      return Enumerable.Empty<TeamCityBuildType>();
    }

    public IEnumerable<TeamCityBuild> GetBuilds(string buildTypeId, string branchName, int start, int count, bool onlySuccessful)
    {
      return Enumerable.Empty<TeamCityBuild>();
    }

    public TeamCityBuild GetBuild(string buildId)
    {
      return new TeamCityBuild
      {
        BranchName = "default",
        BuildTypeId = "buildTypeId",
        Id = buildId,
        Number = "1",
        Pinned = true,
        StartDate = "2015-01-01"
      };
    }

    public TeamCityBuild GetLastSuccessfulBuild(string buildTypeId)
    {
      return new TeamCityBuild
      {
        BranchName = "default",
        BuildTypeId = buildTypeId,
        Id = "buildId",
        Number = "1",
        Pinned = true,
        StartDate = "2015-01-01"
      };
    }

    public void DownloadArtifacts(string buildId, string destinationFilePath)
    {
    }
  }
}