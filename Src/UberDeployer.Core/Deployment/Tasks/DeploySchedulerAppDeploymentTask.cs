using System;
using System.Collections.Generic;
using System.IO;
using NHibernate.Mapping;
using UberDeployer.Common;
using UberDeployer.Common.IO;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;
using UberDeployer.Core.Management.ScheduledTasks;

namespace UberDeployer.Core.Deployment.Tasks
{
  // TODO IMM HI: move common code up
  public class DeploySchedulerAppDeploymentTask : DeploymentTask
  {
    private readonly IArtifactsRepository _artifactsRepository;
    private readonly ITaskScheduler _taskScheduler;
    private readonly IPasswordCollector _passwordCollector;
    // ReSharper disable NotAccessedField.Local
    private readonly IDirectoryAdapter _directoryAdapter;
    // ReSharper restore NotAccessedField.Local
    private readonly IFileAdapter _fileAdapter;
    private readonly IZipFileAdapter _zipFileAdapter;
    
    private SchedulerAppProjectInfo _projectInfo;
    private Dictionary<string, string> _collectedPasswordsByUserName;
    private Dictionary<Tuple<string, string>, ScheduledTaskDetails> _existingTaskDetailsByMachineNameAndTaskName;

    #region Constructor(s)

    public DeploySchedulerAppDeploymentTask(
      IProjectInfoRepository projectInfoRepository,
      IEnvironmentInfoRepository environmentInfoRepository,
      IArtifactsRepository artifactsRepository,
      ITaskScheduler taskScheduler,
      IPasswordCollector passwordCollector,
      IDirectoryAdapter directoryAdapter,
      IFileAdapter fileAdapter,
      IZipFileAdapter zipFileAdapter)
      : base(projectInfoRepository, environmentInfoRepository)
    {
      Guard.NotNull(artifactsRepository, "artifactsRepository");
      Guard.NotNull(taskScheduler, "taskScheduler");
      Guard.NotNull(passwordCollector, "passwordCollector");
      Guard.NotNull(directoryAdapter, "directoryAdapter");
      Guard.NotNull(fileAdapter, "fileAdapter");
      Guard.NotNull(zipFileAdapter, "zipFileAdapter");

      _artifactsRepository = artifactsRepository;
      _taskScheduler = taskScheduler;
      _passwordCollector = passwordCollector;
      _directoryAdapter = directoryAdapter;
      _fileAdapter = fileAdapter;
      _zipFileAdapter = zipFileAdapter;
    }

    #endregion

    #region Overrides of DeploymentTaskBase

    protected override void DoPrepare()
    {
      EnvironmentInfo environmentInfo = GetEnvironmentInfo();

      _projectInfo = GetProjectInfo<SchedulerAppProjectInfo>();
      _collectedPasswordsByUserName = new Dictionary<string, string>();
      _existingTaskDetailsByMachineNameAndTaskName = new Dictionary<Tuple<string, string>, ScheduledTaskDetails>();

      foreach (string tmpSchedulerServerTasksMachineName in environmentInfo.SchedulerServerTasksMachineNames)
      {
        string schedulerServerTasksMachineName = tmpSchedulerServerTasksMachineName;

        _projectInfo.SchedulerAppTasks
          .ForEach(
            schedulerAppTask =>
            {
              ScheduledTaskDetails taskDetails =
                _taskScheduler.GetScheduledTaskDetails(schedulerServerTasksMachineName, schedulerAppTask.Name);

              _existingTaskDetailsByMachineNameAndTaskName.Add(
                Tuple.Create(schedulerServerTasksMachineName, schedulerAppTask.Name),
                taskDetails);

              EnsureTaskIsNotRunning(taskDetails, schedulerServerTasksMachineName);

              // create a step to disable scheduler task
              if (taskDetails != null && taskDetails.IsEnabled)
              {
                AddToggleSchedulerAppTaskEnabledStep(schedulerServerTasksMachineName, taskDetails.Name, false);
              }
            });
      }

      Lazy<string> binariesDirPathProvider =
        AddStepsToObtainBinaries(environmentInfo);

/* // TODO IMM HI: xxx we don't need this for now - should we parameterize this somehow?
      if (_directoryAdapter.Exists(targetDirNetworkPath))
      {
        AddSubTask(
          new BackupFilesDeploymentStep(
            targetDirNetworkPath));
      }
*/

      // create steps for copying the binaries to target binaries machines
      foreach (string schedulerServerBinariesMachineName in environmentInfo.SchedulerServerBinariesMachineNames)
      {
        string targetDirPath = GetTargetDirPath(environmentInfo);
        string targetDirNetworkPath = environmentInfo.GetSchedulerServerNetworkPath(schedulerServerBinariesMachineName, targetDirPath);

        AddSubTask(
          new CleanDirectoryDeploymentStep(
            _directoryAdapter,
            _fileAdapter,
            new Lazy<string>(() => targetDirNetworkPath),
            excludedDirs: new string[] { }));

        AddSubTask(
          new CopyFilesDeploymentStep(
            _directoryAdapter,
            binariesDirPathProvider,
            new Lazy<string>(() => targetDirNetworkPath)));
      }

      foreach (string tmpSchedulerServerTasksMachineName in environmentInfo.SchedulerServerTasksMachineNames)
      {
        string schedulerServerTasksMachineName = tmpSchedulerServerTasksMachineName;

        _projectInfo.SchedulerAppTasks
          .ForEach(
            schedulerAppTask =>
            {
              string taskName = schedulerAppTask.Name;
              
              Tuple<string, string> machineNameAndTaskName =
                Tuple.Create(schedulerServerTasksMachineName, taskName);

              ScheduledTaskDetails existingTaskDetails =
                _existingTaskDetailsByMachineNameAndTaskName[machineNameAndTaskName];

              AddTaskConfigurationSteps(
                environmentInfo,
                schedulerServerTasksMachineName,
                schedulerAppTask,
                existingTaskDetails);

              // create a step to toggle scheduler task enabled
              if (existingTaskDetails == null || existingTaskDetails.IsEnabled)
              {
                AddToggleSchedulerAppTaskEnabledStep(
                  schedulerServerTasksMachineName,
                  taskName,
                  true);
              }
            });
      }
    }

    protected override void DoExecute()
    {
      try
      {
        base.DoExecute();
      }
      catch
      {
        MakeSureTasksThatWereEnabledAreEnabled();

        throw;
      }
    }

    public override string Description
    {
      get
      {
        return
          string.Format(
            "Deploy scheduler app '{0} ({1}:{2})' to '{3}'.",
            DeploymentInfo.ProjectName,
            DeploymentInfo.ProjectConfigurationName,
            DeploymentInfo.ProjectConfigurationBuildId,
            DeploymentInfo.TargetEnvironmentName);
      }
    }

    #endregion

    #region Private methods

    private void AddTaskConfigurationSteps(EnvironmentInfo environmentInfo, string schedulerServerTasksMachineName, SchedulerAppTask schedulerAppTask, ScheduledTaskDetails taskDetails = null)
    {
      TaskSettingsCompareResult settingsCompareResult = CompareTaskSettings(taskDetails, schedulerAppTask, environmentInfo);
      bool hasSettingsChanged = settingsCompareResult.AreEqual == false;      

      bool taskExists = taskDetails != null;

      if (taskExists)
      {
        LogSettingsDifferences(schedulerAppTask.Name, settingsCompareResult);
      }
      else
      {
        PostDiagnosticMessage(string.Format("Scheduler task [{0}] doesn't exist.", schedulerAppTask.Name), DiagnosticMessageType.Trace);
      }

      EnvironmentUser environmentUser =
        environmentInfo.GetEnvironmentUser(schedulerAppTask.UserId);

      string environmentUserPassword = null;

      if (!taskExists || hasSettingsChanged)
      {
        // collect password if not already collected
        if (!_collectedPasswordsByUserName.TryGetValue(environmentUser.UserName, out environmentUserPassword))
        {
          environmentUserPassword =
            PasswordCollectorHelper.CollectPasssword(
              _passwordCollector,
              DeploymentInfo.DeploymentId,
              environmentInfo,
              schedulerServerTasksMachineName,
              environmentUser,
              OnDiagnosticMessagePosted);

          _collectedPasswordsByUserName.Add(environmentUser.UserName, environmentUserPassword);
        }
      }

      string taskExecutablePath =
        GetTaskExecutablePath(schedulerAppTask, environmentInfo);

      if (!taskExists)
      {
        // create a step for scheduling a new app
        AddSubTask(
          new CreateSchedulerTaskDeploymentStep(
            schedulerServerTasksMachineName,
            schedulerAppTask.Name,
            taskExecutablePath,
            environmentUser.UserName,
            environmentUserPassword,
            schedulerAppTask.ScheduledHour,
            schedulerAppTask.ScheduledMinute,
            schedulerAppTask.ExecutionTimeLimitInMinutes,
            schedulerAppTask.Repetition,
            _taskScheduler));
      }
      else if (hasSettingsChanged)
      {
        // create a step for updating an existing scheduler app
        AddSubTask(
          new UpdateSchedulerTaskDeploymentStep(
            schedulerServerTasksMachineName,
            schedulerAppTask.Name,
            taskExecutablePath,
            environmentUser.UserName,
            environmentUserPassword,
            schedulerAppTask.ScheduledHour,
            schedulerAppTask.ScheduledMinute,
            schedulerAppTask.ExecutionTimeLimitInMinutes,
            schedulerAppTask.Repetition,
            _taskScheduler));
      }
    }

    private void LogSettingsDifferences(string taskName, TaskSettingsCompareResult settingsCompareResult)
    {      
      if (settingsCompareResult.AreEqual)
      {
        PostDiagnosticMessage(string.Format("Scheduler task settings and configuration are equal, taskName: [{0}]", taskName), DiagnosticMessageType.Trace);
        return;
      }

      PostDiagnosticMessage(string.Format("Differences between scheduler task settings and configuration for scheduler task: [{0}]", taskName), DiagnosticMessageType.Trace);

      foreach (var settingDifference in settingsCompareResult.Differencies)
      {
        PostDiagnosticMessage(
          string.Format(
            "Scheduler task settings difference, option name: [{0}], current task value: [{1}], configuration value: [{2}]",
            settingDifference.OptionName,
            settingDifference.CurrentTaskValue,
            settingDifference.ExpectedConfigurationValue),
          DiagnosticMessageType.Trace);
      }
    }

    private Lazy<string> AddStepsToObtainBinaries(EnvironmentInfo environmentInfo)
    {
      // create a step for downloading the artifacts
      var downloadArtifactsDeploymentStep =
        new DownloadArtifactsDeploymentStep(
          _projectInfo,
          DeploymentInfo,
          GetTempDirPath(),
          _artifactsRepository);

      AddSubTask(downloadArtifactsDeploymentStep);

      // create a step for extracting the artifacts
      var extractArtifactsDeploymentStep =
        new ExtractArtifactsDeploymentStep(
          _projectInfo, 
          environmentInfo,
          DeploymentInfo,
          downloadArtifactsDeploymentStep.ArtifactsFilePath,
          GetTempDirPath(),
          _fileAdapter,
          _directoryAdapter,
          _zipFileAdapter);

      AddSubTask(extractArtifactsDeploymentStep);

      if (_projectInfo.ArtifactsAreEnvironmentSpecific)
      {
        var binariesConfiguratorStep =
          new ConfigureBinariesStep(
            environmentInfo.ConfigurationTemplateName,
            GetTempDirPath());

        AddSubTask(binariesConfiguratorStep);
      }

      return new Lazy<string>(() => extractArtifactsDeploymentStep.BinariesDirPath);
    }

    private void AddToggleSchedulerAppTaskEnabledStep(string machineName, string taskName, bool enabled)
    {
      AddSubTask(
        new ToggleSchedulerAppEnabledStep(
          _taskScheduler,
          machineName,
          taskName,
          enabled));
    }

    private string GetTargetDirPath(EnvironmentInfo environmentInfo)
    {
      return Path.Combine(environmentInfo.SchedulerAppsBaseDirPath, _projectInfo.SchedulerAppDirName);
    }

    private string GetTaskExecutablePath(SchedulerAppTask schedulerAppTask, EnvironmentInfo environmentInfo)
    {
      string targetDirPath = GetTargetDirPath(environmentInfo);

      return Path.Combine(targetDirPath, schedulerAppTask.ExecutableName);
    }

    private TaskSettingsCompareResult CompareTaskSettings(ScheduledTaskDetails taskDetails, SchedulerAppTask schedulerAppTask, EnvironmentInfo environmentInfo)
    {
      if (taskDetails == null)
      {
        return new TaskSettingsCompareResult(new List<SettingDifference>());
      }

      string taskExecutablePath = GetTaskExecutablePath(schedulerAppTask, environmentInfo);

      var compareResultBuilder = new TaskSettingsCompareResultBuilder();

      compareResultBuilder
        .CompareValues("Name", taskDetails.Name, schedulerAppTask.Name)
        .CompareValues("ScheduledHour", taskDetails.ScheduledHour, schedulerAppTask.ScheduledHour)
        .CompareValues("ScheduledMinute", taskDetails.ScheduledMinute, schedulerAppTask.ScheduledMinute)
        .CompareValues("ExecutionTimeLimitInMinutes", taskDetails.ExecutionTimeLimitInMinutes, schedulerAppTask.ExecutionTimeLimitInMinutes)
        .CompareValues("Repetition.Interval", taskDetails.Repetition.Interval, schedulerAppTask.Repetition.Interval)
        .CompareValues("Repetition.Duration", taskDetails.Repetition.Duration, schedulerAppTask.Repetition.Duration)
        .CompareValues("Repetition.StopAtDurationEnd", taskDetails.Repetition.StopAtDurationEnd, schedulerAppTask.Repetition.StopAtDurationEnd)
        .CompareValues("ExeAbsolutePath", taskDetails.ExeAbsolutePath, taskExecutablePath);

      return compareResultBuilder.GetResult();
    }

    private void MakeSureTasksThatWereEnabledAreEnabled()
    {
      foreach (Tuple<string, string> machineNameAndTaskName in _existingTaskDetailsByMachineNameAndTaskName.Keys)
      {
        string schedulerServerTasksMachineName = machineNameAndTaskName.Item1;
        string taskName = machineNameAndTaskName.Item2;

        try
        {
          ScheduledTaskDetails existingTaskDetails =
            _existingTaskDetailsByMachineNameAndTaskName[machineNameAndTaskName];

          if (existingTaskDetails == null || existingTaskDetails.IsEnabled)
          {
            ScheduledTaskDetails currentTaskDetails =
              _taskScheduler.GetScheduledTaskDetails(schedulerServerTasksMachineName, taskName);

            if (currentTaskDetails != null && !currentTaskDetails.IsEnabled)
            {
              _taskScheduler.ToggleTaskEnabled(schedulerServerTasksMachineName, taskName, true);
            }
          }
        }
        catch (Exception exc)
        {
          PostDiagnosticMessage(string.Format("Error while making sure that tasks that were enabled are enabled on machine '{0}'. Exception: {1}", schedulerServerTasksMachineName, exc), DiagnosticMessageType.Error);
        }
      }
    }

    private static void EnsureTaskIsNotRunning(ScheduledTaskDetails taskDetails, string schedulerServerTasksMachineName)
    {
      if (taskDetails == null || !taskDetails.IsRunning)
      {
        return;
      }

      throw
        new DeploymentTaskException(
          string.Format(
            "Task: {0} on machine: {1} is already running. Deployment aborted. Last run time: {2}, next run time: {3}",
            schedulerServerTasksMachineName,
            taskDetails.Name,
            taskDetails.LastRunTime,
            taskDetails.NextRunTime));
    }

    #endregion

    private class TaskSettingsCompareResultBuilder
    {
      public TaskSettingsCompareResultBuilder()
      {
        _differencies = new List<SettingDifference>();
      }

      private readonly List<SettingDifference> _differencies;

      public TaskSettingsCompareResultBuilder CompareValues<T>(string optionName, T currentValue, T expectedValue)
      {
        if (currentValue.Equals(expectedValue) == false)
        {
          _differencies.Add(new SettingDifference(optionName, currentValue.ToString(), expectedValue.ToString()));;
        }

        return this;
      }

      public TaskSettingsCompareResult GetResult()
      {
        return new TaskSettingsCompareResult(_differencies);
      }
    }

    private class TaskSettingsCompareResult
    {
      public TaskSettingsCompareResult(List<SettingDifference> differences)
      {
        Differencies = differences;
      }

      public List<SettingDifference> Differencies { get; private set; }

      public bool AreEqual
      {
        get { return Differencies.Count == 0; }
      }      
    }

    private class SettingDifference
    {
      public SettingDifference(string optionName, string currentTaskValue, string expectedConfigurationValue)
      {
        OptionName = optionName;
        CurrentTaskValue = currentTaskValue;
        ExpectedConfigurationValue = expectedConfigurationValue;
      }

      public string OptionName { get; private set; }

      public string CurrentTaskValue { get; private set; }

      public string ExpectedConfigurationValue { get; private set; }
    }
  }
}
