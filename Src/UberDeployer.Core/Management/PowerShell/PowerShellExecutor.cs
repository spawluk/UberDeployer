using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.PowerShell
{
  public class PowerShellExecutor : IPowerShellExecutor
  {
    private readonly PowerShellConfiguration _powerShellConfiguration;

    private int _errorCount = 0;

    public PowerShellExecutor(PowerShellConfiguration powerShellConfiguration)
    {
      Guard.NotNull(powerShellConfiguration, "powerShellConfiguration");

      _powerShellConfiguration = powerShellConfiguration;
    }

    public PSObject Execute(string script)
    {
      using (Runspace runspace = CreateRunspace())
      {
        runspace.Open();

        using (var powerShell = System.Management.Automation.PowerShell.Create())
        {
          powerShell.Runspace = runspace;

          powerShell.AddScript(script);

          powerShell.Streams.Error.DataAdded += OnError;
          powerShell.Streams.Debug.DataAdded += OnDebug;
          powerShell.Streams.Warning.DataAdded += OnWarning;
          powerShell.Streams.Progress.DataAdded += OnProgress;
          powerShell.Streams.Verbose.DataAdded += OnVerbose;

          var outputCollection = new PSDataCollection<PSObject>();

          outputCollection.DataAdded += OnOutput;

          IAsyncResult invokeResult = powerShell.BeginInvoke<PSObject, PSObject>(null, outputCollection);

          powerShell.EndInvoke(invokeResult);

          if (_errorCount != 0)
          {
            throw new PowerShellScriptExecutionException(powerShell.Streams.Error);
          }

          return outputCollection.LastOrDefault();
        }
      }
    }

    private Runspace CreateRunspace()
    {
      if (_powerShellConfiguration.IsRemote)
      {
        var connectionInfo = new WSManConnectionInfo
        {
          ComputerName = _powerShellConfiguration.RemoteMachineName,
          AuthenticationMechanism = AuthenticationMechanism.Negotiate,
        };

        return RunspaceFactory.CreateRunspace(connectionInfo);
      }

      return RunspaceFactory.CreateRunspace();
    }

    private void OnOutput(object sender, DataAddedEventArgs e)
    {
      WriteOutput("PS", sender, e);
    }

    private void OnVerbose(object sender, DataAddedEventArgs e)
    {
      WriteOutput("PS VERBOSE: ", sender, e);
    }

    private void OnProgress(object sender, DataAddedEventArgs e)
    {
      WriteOutput("PS PROGRESS", sender, e);
    }

    private void OnWarning(object sender, DataAddedEventArgs e)
    {
      WriteOutput("PS WARNING", sender, e);
    }

    private void OnDebug(object sender, DataAddedEventArgs e)
    {
      WriteOutput("PS DEBUG", sender, e);
    }

    private void OnError(object sender, DataAddedEventArgs e)
    {
      _errorCount++;
      WriteError("PS ERROR", sender ,e);
    }

    private void WriteOutput(string prefix, object sender, DataAddedEventArgs e)
    {
      if (_powerShellConfiguration.OnOutput == null)
      {
        return;
      }

      var psDataCollection = sender as PSDataCollection<PSObject>;
      if (psDataCollection != null)
      {
        PSObject psObject = psDataCollection[e.Index];
        
        _powerShellConfiguration.OnOutput(string.Format("{0}: {1}", prefix, psObject));
      }
    }

    private void WriteError(string prefix, object sender, DataAddedEventArgs e)
    {
      if (_powerShellConfiguration.OnError == null)
      {
        return;
      }

      var psErrorCollection = sender as PSDataCollection<ErrorRecord>;
      if (psErrorCollection != null)
      {
        ErrorRecord error = psErrorCollection[e.Index];
        _powerShellConfiguration.OnError(string.Format("{0}: {1}", prefix, error.Exception));
      }
    }
  }
}
