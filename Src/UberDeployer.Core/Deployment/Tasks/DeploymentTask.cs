﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberDeployer.Common;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Tasks.DependenciesDeployment;
using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Deployment.Tasks
{
  public abstract class DeploymentTask : DeploymentTaskBase
  {
    private readonly IProjectInfoRepository _projectInfoRepository;
    private readonly IEnvironmentInfoRepository _environmentInfoRepository;
    private readonly List<DeploymentTaskBase> _subTasks;

    private DeploymentInfo _deploymentInfo;
    private string _tempDirPath;

    protected DeploymentTask(IProjectInfoRepository projectInfoRepository, IEnvironmentInfoRepository environmentInfoRepository)
    {      
      Guard.NotNull(projectInfoRepository, "projectInfoRepository");
      Guard.NotNull(environmentInfoRepository, "environmentInfoRepository");

      _projectInfoRepository = projectInfoRepository;
      _environmentInfoRepository = environmentInfoRepository;

      _subTasks = new List<DeploymentTaskBase>();
    }

    public void EnableDependenciesDeployment(IObjectFactory objectFactory)
    {
      if (IsPrepared)
      {
        throw new InvalidOperationException("Task is already prepared.");
      }

      if (_deploymentInfo == null)
      {
        throw new InvalidOperationException("Task is not initialized.");
      }

      AddSubTask(
        new DeployDependenciesTask(
            _deploymentInfo.ProjectName,
            _deploymentInfo.TargetEnvironmentName,
            _deploymentInfo.DeploymentId,
            _projectInfoRepository,
            objectFactory,
            objectFactory.CreateTeamCityRestClient(),
            objectFactory.CreateDependentProjectsToDeployWebSelector()));
    }

    public void Initialize(DeploymentInfo deploymentInfo)
    {
      Guard.NotNull(deploymentInfo, "deploymentInfo");

      _deploymentInfo = deploymentInfo;      
    }

    protected override void DoPrepare()
    {
      // do nothing
    }

    protected override void DoExecute()
    {
      try
      {
        if (!DeploymentInfo.IsSimulation)
        {
          PostDiagnosticMessage(string.Format("Executing: {0}", Description), DiagnosticMessageType.Info);
        }                

        foreach (DeploymentTaskBase subTask in _subTasks)
        {
          var deploymentTask = subTask as DeploymentTask;

          if (deploymentTask != null)
          {
            deploymentTask.Initialize(DeploymentInfo);
          }

          subTask.Prepare();

          if (!DeploymentInfo.IsSimulation)
          {
            subTask.Execute();
          }
        }

        if (DeploymentInfo.IsSimulation)
        {
          Simulate();
        }
      }
      finally
      {
        DeleteTemporaryDirectoryIfNeeded();
      }
    }

    public override string Description
    {
      get { return string.Join(Environment.NewLine, _subTasks.Select(st => st.Description).ToArray()); }
    }

    protected EnvironmentInfo GetEnvironmentInfo()
    {
      EnvironmentInfo environmentInfo =
        _environmentInfoRepository.FindByName(
          DeploymentInfo.TargetEnvironmentName);

      if (environmentInfo == null)
      {
        throw new DeploymentTaskException(string.Format("Environment named '{0}' doesn't exist.", DeploymentInfo.TargetEnvironmentName));
      }

      return environmentInfo;
    }

    protected T GetProjectInfo<T>()
      where T : ProjectInfo
    {
      ProjectInfo projectInfo =
        _projectInfoRepository.FindByName(DeploymentInfo.ProjectName);

      if (projectInfo == null)
      {
        throw new DeploymentTaskException(string.Format("Project named '{0}' doesn't exist.", DeploymentInfo.ProjectName));
      }

      if (!(projectInfo is T))
      {
        throw new DeploymentTaskException(string.Format("Project named '{0}' is not of the expected type: '{1}'.", DeploymentInfo.ProjectName, typeof(T).FullName));
      }

      return (T)projectInfo;
    }

    protected T GetProjectInfo<T>(string projectName)
      where T : ProjectInfo
    {
      ProjectInfo projectInfo =
        _projectInfoRepository.FindByName(projectName);

      if (projectInfo == null)
      {
        throw new DeploymentTaskException(string.Format("Project named '{0}' doesn't exist.", DeploymentInfo.ProjectName));
      }

      if (!(projectInfo is T))
      {
        throw new DeploymentTaskException(string.Format("Project named '{0}' is not of the expected type: '{1}'.", DeploymentInfo.ProjectName, typeof(T).FullName));
      }

      return (T)projectInfo;
    }

    protected List<T> GetProjects<T>()
    {
      return _projectInfoRepository.GetAll()
        .OfType<T>()
        .ToList();
    }

    protected void AddSubTask(DeploymentTaskBase subTask)
    {
      if (subTask == null)
      {
        throw new ArgumentNullException("subTask");
      }

      _subTasks.Add(subTask);

      // this will cause the events raised by sub-tasks to bubble up
      subTask.DiagnosticMessagePosted += OnDiagnosticMessagePosted;
    }

    protected virtual void Simulate()
    {
      // do nothing
    }

    protected string GetTempDirPath()
    {
      if (string.IsNullOrEmpty(_tempDirPath))
      {
        string tempDirName = Guid.NewGuid().ToString("N");

        _tempDirPath = Path.Combine(Path.GetTempPath(), tempDirName);

        Directory.CreateDirectory(_tempDirPath);
      }

      return _tempDirPath;
    }

    protected DeploymentInfo DeploymentInfo
    {
      get
      {
        if (_deploymentInfo == null)
        {
          throw new InvalidOperationException("DeploymentInfo is missing - have you initialized the task?");
        }

        return _deploymentInfo;
      }
    }

    private void DeleteTemporaryDirectoryIfNeeded()
    {
      if (!string.IsNullOrEmpty(_tempDirPath) && Directory.Exists(_tempDirPath))
      {
        RetryUtils.RetryOnException(
          new[] { typeof(IOException) },
          retriesCount: 4,
          retryDelay: 500,
          action: () => Directory.Delete(_tempDirPath, true));
      }
    }

    public IEnumerable<DeploymentTaskBase> SubTasks
    {
      get { return _subTasks.AsReadOnly(); }
    }
  }
}