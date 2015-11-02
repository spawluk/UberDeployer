using UberDeployer.Common.IO;
using UberDeployer.Core.Deployment.Steps;
using UberDeployer.Core.Domain;

namespace UberDeployer.Core.Deployment.Tasks
{
  public class DeployPowerShellScriptDeploymentTask : DeploymentTask
  {
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
      _artifactsRepository = artifactsRepository;
      _fileAdapter = fileAdapter;
      _directoryAdapter = directoryAdapter;
      _zipFileAdapter = zipFileAdapter;
    }

    protected override void DoPrepare()
    {
      EnvironmentInfo environmentInfo = GetEnvironmentInfo();

      _projectInfo = GetProjectInfo<PowerShellScriptProjectInfo>();

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

      var runPowerShellScriptStep = 
        new RunPowerShellScriptStep(
        _projectInfo.MachineName,
        _projectInfo.ScriptPath,
        _fileAdapter);

      AddSubTask(runPowerShellScriptStep);
    }
  }
}
