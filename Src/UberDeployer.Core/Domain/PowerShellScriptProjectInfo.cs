using System.Collections.Generic;
using log4net.Core;
using NHibernate.Linq.Functions;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.Deployment.Tasks;
using UberDeployer.Core.Domain.Input;

namespace UberDeployer.Core.Domain
{
  public class PowerShellScriptProjectInfo : ProjectInfo
  {
    public string _scriptPath;

    public PowerShellScriptProjectInfo(
      string name, 
      string artifactsRepositoryName, 
      IEnumerable<string> allowedEnvironmentNames, 
      string artifactsRepositoryDirName, 
      bool artifactsAreNotEnvironmentSpecific, 
      string scriptPath, 
      List<string> dependendProjectNames = null)
      : base(
        name, 
        artifactsRepositoryName, 
        allowedEnvironmentNames, 
        dependendProjectNames, 
        artifactsRepositoryDirName, 
        artifactsAreNotEnvironmentSpecific)
    {
      Guard.NotNullNorEmpty(scriptPath, "scriptPath");

      _scriptPath = scriptPath;
    }

    public override ProjectType Type
    {
      get { return ProjectType.PowerShellScript; }
    }

    public override InputParams CreateEmptyInputParams()
    {
      return new PowerShellInputParams();
    }

    public override DeploymentTask CreateDeploymentTask(IObjectFactory objectFactory)
    {
      return
        new DeployPowerShellScriptDeploymentTask(
          objectFactory.CreateProjectInfoRepository(),
          objectFactory.CreateEnvironmentInfoRepository(),
          objectFactory.CreateArtifactsRepository(),
          objectFactory.CreateFileAdapter(),
          objectFactory.CreateDirectoryAdapter(),
          objectFactory.CreateZipFileAdapter());
    }

    public override IEnumerable<string> GetTargetFolders(IObjectFactory objectFactory, EnvironmentInfo environmentInfo)
    {
      throw new System.NotImplementedException();
    }

    public override string GetMainAssemblyFileName()
    {
      throw new System.NotImplementedException();
    }

    public ExecuteOnMachine ExecuteOnMachine { get; private set; }

    public IEnumerable<string> GetTargetMachines(EnvironmentInfo environmentInfo)
    {
      if (ExecuteOnMachine == null || ExecuteOnMachine.TargetMachine == null)
      {
        throw new DeploymentTaskException("Target machine to run PowerShell script is not specified. Set 'ExecuteOnMachine' property in ProjectInfos.xml");
      }

      var appServerMachine = ExecuteOnMachine.TargetMachine as AppServerTargetMachine;
      if (appServerMachine != null)
      {
        return new [] { environmentInfo.AppServerMachineName };
      }

      var webServerMachines = ExecuteOnMachine.TargetMachine as WebServerTargetMachines;
      if (webServerMachines != null)
      {
        return environmentInfo.WebServerMachineNames;
      }

      var terminalServerMachine = ExecuteOnMachine.TargetMachine as TerminalServerTargetMachine;
      if (terminalServerMachine != null)
      {
        return new [] { environmentInfo.TerminalServerMachineName };
      }

      var schedulerServerMachines = ExecuteOnMachine.TargetMachine as SchedulerServerTargetMachines;
      if (schedulerServerMachines != null)
      {
        return environmentInfo.SchedulerServerTasksMachineNames;
      }

      var databaseServerMachine = ExecuteOnMachine.TargetMachine as DatabaseServerTargetMachine;
      if (databaseServerMachine != null)
      {
        return new [] { environmentInfo.GetDatabaseServer(databaseServerMachine.DatabaseServerId).MachineName };
      }

      throw new DeploymentTaskException(string.Format("Target Machine type is not supported [{0}]", ExecuteOnMachine.TargetMachine));
    }
  }

  public class ExecuteOnMachine
  {
    public TargetMachine TargetMachine { get; set; }
  }

  public abstract class TargetMachine { }

  public class AppServerTargetMachine : TargetMachine
  {
  }

  public class WebServerTargetMachines : TargetMachine { }

  public class TerminalServerTargetMachine : TargetMachine { }

  public class SchedulerServerTargetMachines : TargetMachine { }

  public class DatabaseServerTargetMachine : TargetMachine
  {
    public string DatabaseServerId { get; set; }
  }
}
