using System;
using System.Collections.Generic;
using System.Threading;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.DataAccess.WebClient;
using UberDeployer.Core.Deployment;

namespace UberDeployer.Core.ExternalDataCollectors.DependentProjectsSelection
{
  public class DependentProjectsToDeployWebSelector : IDependentProjectsToDeployWebSelector
  {
    private static readonly Dictionary<Guid, DependentProjectsToDeploySelectionResult> _collectedProjectsByDeploymentId = new Dictionary<Guid, DependentProjectsToDeploySelectionResult>();
    
    private readonly IInternalApiWebClient _internalApiWebClient;
    private readonly int _maxWaitTimeInSeconds;

    public event EventHandler<DiagnosticMessageEventArgs> DiagnosticMessagePosted;

    public DependentProjectsToDeployWebSelector(IInternalApiWebClient internalApiWebClient, int maxWaitTimeInSeconds)
    {
      Guard.NotNull(internalApiWebClient, "internalApiWebClient");
      
      _internalApiWebClient = internalApiWebClient;
      _maxWaitTimeInSeconds = maxWaitTimeInSeconds;
    }

    public static void SetSelectedProjectsToDeploy(Guid deploymentId, DependentProjectsToDeploySelection dependentProjectsToDeploySelection)
    {
      lock (_collectedProjectsByDeploymentId)
      {
        _collectedProjectsByDeploymentId[deploymentId] = new DependentProjectsToDeploySelectionResult
        {
          DependentProjectsToDeploySelection = dependentProjectsToDeploySelection
        };
      }
    }

    public DependentProjectsToDeploySelection GetSelectedProjectsToDeploy(Guid deploymentId, string userName, List<DependentProject> dependentProjects)
    {
      _internalApiWebClient.CollectDependenciesToDeploy(deploymentId, userName, dependentProjects);

      var pollStartTime = DateTime.UtcNow;
      DependentProjectsToDeploySelectionResult dependentProjectsToDeploySelectionResult;

      while (true)
      {
        PostDiagnosticMessage("Waiting for script selection...", DiagnosticMessageType.Trace);

        lock (_collectedProjectsByDeploymentId)
        {
          if (_collectedProjectsByDeploymentId.TryGetValue(deploymentId, out dependentProjectsToDeploySelectionResult))
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

      if (dependentProjectsToDeploySelectionResult != null && dependentProjectsToDeploySelectionResult.Canceled)
      {
        PostDiagnosticMessage("Canceled selection of scripts to run - we'll not continue.", DiagnosticMessageType.Trace);

        return new DependentProjectsToDeploySelection();
      }

      if (dependentProjectsToDeploySelectionResult == null || dependentProjectsToDeploySelectionResult.DependentProjectsToDeploySelection.SelectedProjects == null || dependentProjectsToDeploySelectionResult.DependentProjectsToDeploySelection.SelectedProjects.Count == 0)
      {
        _internalApiWebClient.OnCollectDependenciesToDeployTimedOut(deploymentId);
        
        throw new TimeoutException("Given up waiting for scripts selection.");
      }

      PostDiagnosticMessage("Scripts to run were provided - we'll continue.", DiagnosticMessageType.Trace);

      return dependentProjectsToDeploySelectionResult.DependentProjectsToDeploySelection;
    }

    public void PostDiagnosticMessage(string message, DiagnosticMessageType diagnosticMessageType)
    {
      if (string.IsNullOrEmpty(message))
      {
        throw new ArgumentException("Argument can't be null nor empty.", "message");
      }

      OnDiagnosticMessagePosted(this, new DiagnosticMessageEventArgs(diagnosticMessageType, message));
    }

    public void OnDiagnosticMessagePosted(object sender, DiagnosticMessageEventArgs diagnosticMessageEventArgs)
    {
      var eventHandler = DiagnosticMessagePosted;

      if (eventHandler != null)
      {
        eventHandler(sender, diagnosticMessageEventArgs);
      }
    }

    public static void CancelDbScriptsSelection(Guid deploymentId)
    {
      lock (_collectedProjectsByDeploymentId)
      {
        _collectedProjectsByDeploymentId[deploymentId] = new DependentProjectsToDeploySelectionResult
        {
          Canceled = true
        };
      }
    }
  }
}
