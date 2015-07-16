using System.Collections.Generic;

namespace UberDeployer.Core.Domain
{
  public interface IProjectInfoRepository
  {
    IEnumerable<ProjectInfo> GetAll();

    ProjectInfo FindByName(string name);

    List<ProjectInfo> FindProjectNameWithDependencies(string name);

    List<ProjectInfo> FindProjectNameWithDependencies(ProjectInfo projectInfo);

    List<ProjectInfo> CreateDependentProjects(string name);

    List<ProjectInfo> CreateDependendProjects(ProjectInfo info);
  }
}
