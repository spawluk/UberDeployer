﻿using System;
using System.Collections.Generic;
using UberDeployer.Agent.Proxy.Dto;
using UberDeployer.Agent.Proxy.Dto.EnvDeployment;
using UberDeployer.Agent.Proxy.Dto.Metadata;
using UberDeployer.Agent.Proxy.Dto.TeamCity;

namespace UberDeployer.Agent.Proxy
{
  public class AgentServiceClient : WcfProxy<IAgentService>, IAgentService
  {
    #region IAgentService Members

    public void Deploy(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfo)
    {
      Exec(@as => @as.Deploy(deploymentId, uniqueClientId, requesterIdentity, deploymentInfo));
    }

    public void DeployAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfoDto)
    {
      Exec(@as => @as.DeployAsync(deploymentId, uniqueClientId, requesterIdentity, deploymentInfoDto));
    }

    public void CreatePackageAsync(Guid deploymentId, Guid uniqueClientId, string requesterIdentity, DeploymentInfo deploymentInfo, string packageDirPath)
    {
      Exec(@as => @as.CreatePackageAsync(deploymentId, uniqueClientId, requesterIdentity, deploymentInfo, packageDirPath));
    }

    public List<ProjectInfo> GetProjectInfos(ProjectFilter projectFilter)
    {
      return Exec(@as => @as.GetProjectInfos(projectFilter));
    }

    public List<EnvironmentInfo> GetEnvironmentInfos()
    {
      return Exec(@as => @as.GetEnvironmentInfos());
    }

    public List<ProjectConfiguration> GetProjectConfigurations(string projectName, ProjectConfigurationFilter projectConfigurationFilter)
    {
      return Exec(@as => @as.GetProjectConfigurations(projectName, projectConfigurationFilter));
    }

    public List<ProjectConfigurationBuild> GetProjectConfigurationBuilds(string projectName, string projectConfigurationName, int maxCount, ProjectConfigurationBuildFilter projectConfigurationBuildFilter)
    {
      return Exec(@as => @as.GetProjectConfigurationBuilds(projectName, projectConfigurationName, maxCount, projectConfigurationBuildFilter));
    }

    public List<string> GetWebAppProjectTargetUrls(string projectName, string environmentName)
    {
      return Exec(@as => @as.GetWebAppProjectTargetUrls(projectName, environmentName));
    }

    public List<string> GetProjectTargetFolders(string projectName, string environmentName)
    {
      return Exec(@as => @as.GetProjectTargetFolders(projectName, environmentName));
    }

    public List<DeploymentRequest> GetDeploymentRequests(int startIndex, int maxCount)
    {
      return Exec(@as => @as.GetDeploymentRequests(startIndex, maxCount));
    }

    public List<DiagnosticMessage> GetDiagnosticMessages(Guid uniqueClientId, long lastSeenMaxMessageId, DiagnosticMessageType minMessageType)
    {
      return Exec(@as => @as.GetDiagnosticMessages(uniqueClientId, lastSeenMaxMessageId, minMessageType));
    }

    public List<string> GetWebMachineNames(string environmentName)
    {
      return Exec(@as => @as.GetWebMachineNames(environmentName));
    }

    public ProjectMetadata GetProjectMetadata(string projectName, string environmentName)
    {
      return Exec(@as => @as.GetProjectMetadata(projectName, environmentName));
    }

    public void SetCollectedCredentialsForAsynchronousWebCredentialsCollector(Guid deploymentId, string password)
    {
      Exec(@as => @as.SetCollectedCredentialsForAsynchronousWebCredentialsCollector(deploymentId, password));
    }

    public string GetDefaultPackageDirPath(string environmentName, string projectName)
    {
      return Exec(@as => @as.GetDefaultPackageDirPath(environmentName, projectName));
    }

    public List<string> GetProjectsForEnvironmentDeploy(string environmentName)
    {
      return Exec(@as => @as.GetProjectsForEnvironmentDeploy(environmentName));
    }

    public void DeployEnvironmentAsync(Guid uniqueClientId, string requesterIdentity, string targetEnvironment, List<ProjectToDeploy> projects)    
    {
      Exec(@as => @as.DeployEnvironmentAsync(uniqueClientId, requesterIdentity, targetEnvironment, projects));
    }

    #endregion
  }
}
