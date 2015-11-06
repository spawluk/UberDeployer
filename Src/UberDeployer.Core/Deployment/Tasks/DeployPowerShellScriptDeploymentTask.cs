using System;
using System.Collections.Generic;
using UberDeployer.Common.IO;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Deployment.Tasks
{
  public class DeployPowerShellScriptDeploymentTask : DeploymentTask
  {
    public const string ScriptName = "Install.ps1";

    private readonly IFileAdapter _fileAdapter;
    private readonly IArtifactsRepository _artifactsRepository;
    private readonly IDirectoryAdapter _directoryAdapter;
    private readonly IZipFileAdapter _zipFileAdapter;

    private PowerShellScriptProjectInfo _projectInfo;

    public DeployPowerShellScriptDeploymentTask(
      IProjectInfoRepository projectInfoRepository, 
      IEnvironmentInfoRepository environmentInfoRepository,
      IArtifactsRepository artifactsRepository,
      IFileAdapter fileAdapter,
      IDirectoryAdapter directoryAdapter, 
      IZipFileAdapter zipFileAdapter) 
      : base(projectInfoRepository, environmentInfoRepository)
    {
      Guard.NotNull(artifactsRepository, "artifactsRepository");
      Guard.NotNull(fileAdapter, "fileAdapter");
      Guard.NotNull(directoryAdapter, "directoryAdapter");
      Guard.NotNull(zipFileAdapter, "zipFileAdapter");

      _artifactsRepository = artifactsRepository;
      _fileAdapter = fileAdapter;
      _directoryAdapter = directoryAdapter;
      _zipFileAdapter = zipFileAdapter;
    }

    protected override void DoPrepare()
    {
      EnvironmentInfo environmentInfo = GetEnvironmentInfo();
      _projectInfo = GetProjectInfo<PowerShellScriptProjectInfo>();

      IEnumerable<string> targetMachineNames = _projectInfo.GetTargetMachines(environmentInfo);

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

      foreach (var targetMachineName in targetMachineNames)
      {
        // Create temp dir on remote machine
        var createRemoteTempDirStep = new CreateRemoteTempDirStep(targetMachineName);

        AddSubTask(createRemoteTempDirStep);

        // Copy files to remote machine
        string machineName = targetMachineName;

        var copyFilesDeploymentStep = new CopyFilesDeploymentStep(
          _directoryAdapter,
          srcDirPathProvider : new Lazy<string>(() => extractArtifactsDeploymentStep.BinariesDirPath),
          dstDirPath: new Lazy<string>(() => EnvironmentInfo.GetNetworkPath(machineName, createRemoteTempDirStep.RemoteTempDirPath)));

        AddSubTask(copyFilesDeploymentStep);

        // Run powershell script
        var runPowerShellScriptStep =
          new RunPowerShellScriptStep(
            targetMachineName,
            new Lazy<string>(() => createRemoteTempDirStep.RemoteTempDirPath),
            ScriptName);

        AddSubTask(runPowerShellScriptStep);

        // Delete remote temp dir
        var removeRemoteDirectory = new RemoveRemoteDirectoryStep(
          targetMachineName,
          new Lazy<string>(() => createRemoteTempDirStep.RemoteTempDirPath));

        AddSubTask(removeRemoteDirectory);
      }
    }

    public override string Description
    {
      get
      {
        return string.Format("Deploy PowerShell script project [{0}]", DeploymentInfo.ProjectName);
      }
    }
  }
}
