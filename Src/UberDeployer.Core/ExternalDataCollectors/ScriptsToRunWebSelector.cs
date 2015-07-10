using System;
using System.Collections.Generic;
using System.Threading;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.DataAccess.WebClient;
using UberDeployer.Core.Deployment;
using UberDeployer.Core.Deployment.Steps;

namespace UberDeployer.Core.ExternalDataCollectors
{
  internal class DbScriptsToRunSelectionResult
  {
    public bool Canceled { get; set; }

    public DbScriptsToRunSelection DbScriptsToRunSelection { get; set; }
  }

  public class ScriptsToRunSelector : IScriptsToRunSelector
  {
    private static readonly Dictionary<Guid, DbScriptsToRunSelectionResult> _collectedScriptsByDeploymentId = new Dictionary<Guid, DbScriptsToRunSelectionResult>();

    private readonly IInternalApiWebClient _internalApiWebClient;
    
    private readonly int _maxWaitTimeInSeconds;

    public event EventHandler<DiagnosticMessageEventArgs> DiagnosticMessagePosted;

    public ScriptsToRunSelector(IInternalApiWebClient internalApiWebClient, int maxWaitTimeInSeconds)
    {
      Guard.NotNull(internalApiWebClient, "internalApiWebClient");

      _internalApiWebClient = internalApiWebClient;
      _maxWaitTimeInSeconds = maxWaitTimeInSeconds;
    }

    public static void SetSelectedScriptsToRun(Guid deploymentId, DbScriptsToRunSelection dbScriptsToRunSelection)
    {
      lock (_collectedScriptsByDeploymentId)
      {
        _collectedScriptsByDeploymentId[deploymentId] = new DbScriptsToRunSelectionResult
        {
          DbScriptsToRunSelection = dbScriptsToRunSelection
        };
      }
    }

    public DbScriptsToRunSelection GetSelectedScriptsToRun(Guid deploymentId, string[] sourceScriptsList)
    {
      _internalApiWebClient.CollectScriptsToRun(deploymentId, sourceScriptsList);

      var pollStartTime = DateTime.UtcNow;
      DbScriptsToRunSelectionResult scriptsToRunSelection;

      while (true)
      {
        PostDiagnosticMessage("Waiting for script selection...", DiagnosticMessageType.Trace);

        lock (_collectedScriptsByDeploymentId)
        {
          if (_collectedScriptsByDeploymentId.TryGetValue(deploymentId, out scriptsToRunSelection))
          {
            break;
          }
        }

        Thread.Sleep(1000);

        if (DateTime.UtcNow - pollStartTime > new TimeSpan(0, 0, _maxWaitTimeInSeconds))
        {
          PostDiagnosticMessage("No scripts were selected in the alloted time slot - we'll time out.", DiagnosticMessageType.Trace);

          break;
        }
      }

      if (scriptsToRunSelection != null && scriptsToRunSelection.Canceled)
      {
        PostDiagnosticMessage("Canceled selection of scripts to run - we'll not continue.", DiagnosticMessageType.Trace);

        return new DbScriptsToRunSelection();
      }

      if (scriptsToRunSelection == null || scriptsToRunSelection.DbScriptsToRunSelection.SelectedScripts == null || scriptsToRunSelection.DbScriptsToRunSelection.SelectedScripts.Length == 0)
      {
        _internalApiWebClient.OnCollectScriptsToRunTimedOut(deploymentId);
        
        throw new TimeoutException("Given up waiting for scripts selection.");
      }

      PostDiagnosticMessage("Scripts to run were provided - we'll continue.", DiagnosticMessageType.Trace);

      return scriptsToRunSelection.DbScriptsToRunSelection;
    }

    private void PostDiagnosticMessage(string message, DiagnosticMessageType diagnosticMessageType)
    {
      if (string.IsNullOrEmpty(message))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "message");
      }

      OnDiagnosticMessagePosted(this, new DiagnosticMessageEventArgs(diagnosticMessageType, message));
    }

    private void OnDiagnosticMessagePosted(object sender, DiagnosticMessageEventArgs diagnosticMessageEventArgs)
    {
      var eventHandler = DiagnosticMessagePosted;

      if (eventHandler != null)
      {
        eventHandler(sender, diagnosticMessageEventArgs);
      }
    }

    public static void CancelDbScriptsSelection(Guid deploymentId)
    {
      lock (_collectedScriptsByDeploymentId)
      {
        _collectedScriptsByDeploymentId[deploymentId] = new DbScriptsToRunSelectionResult
        {
          Canceled = true
        };
      }
    }
  }
} 