using System;
using System.Collections.Generic;
using System.Threading;
using UberDeployer.Common.SyntaxSugar;
using UberDeployer.Core.DataAccess.WebClient;
using UberDeployer.Core.Deployment;

namespace UberDeployer.Core.ExternalDataCollectors
{
  public class AsynchronousWebPasswordCollector : IPasswordCollector
  {
    public event EventHandler<DiagnosticMessageEventArgs> DiagnosticMessagePosted;

    private static readonly Dictionary<Guid, string> _collectedPasswordByDeploymentId = new Dictionary<Guid, string>();

    private readonly IInternalApiWebClient _internalApiWebClient;

    private readonly int _maxWaitTimeInSeconds;

    public AsynchronousWebPasswordCollector(IInternalApiWebClient internalApiWebClient, int maxWaitTimeInSeconds)
    {
      Guard.NotNull(internalApiWebClient, "internalApiWebClient");

      _maxWaitTimeInSeconds = maxWaitTimeInSeconds;
      _internalApiWebClient = internalApiWebClient;
    }

    public static void SetCollectedCredentials(Guid deploymentId, string password)
    {
      Guard.NotEmpty(deploymentId, "deploymentId");
      Guard.NotNullNorEmpty(password, "password");

      lock (_collectedPasswordByDeploymentId)
      {
        _collectedPasswordByDeploymentId[deploymentId] = password;
      }
    }

    public string CollectPasswordForUser(Guid deploymentId, string environmentName, string machineName, string userName)
    {
      Guard.NotNullNorEmpty(environmentName, "environmentName");
      Guard.NotNullNorEmpty(machineName, "machineName");
      Guard.NotNullNorEmpty(userName, "userName");

      _internalApiWebClient.CollectCredentials(deploymentId, environmentName, machineName, userName);

      DateTime pollStartTime = DateTime.UtcNow;
      string password;

      while (true)
      {
        PostDiagnosticMessage("Waiting for credentials...", DiagnosticMessageType.Trace);

        lock (_collectedPasswordByDeploymentId)
        {
          if (_collectedPasswordByDeploymentId.TryGetValue(deploymentId, out password))
          {
            break;
          }
        }

        Thread.Sleep(1000);

        if (DateTime.UtcNow - pollStartTime > new TimeSpan(0, 0, _maxWaitTimeInSeconds))
        {
          PostDiagnosticMessage("No credentials were provided in the alloted time slot - we'll time out.", DiagnosticMessageType.Trace);

          break;
        }
      }

      if (string.IsNullOrEmpty(password))
      {
        _internalApiWebClient.OnCollectCredentialsTimedOut(deploymentId);
        
        throw new TimeoutException("Given up waiting for credentials.");
      }

      PostDiagnosticMessage("Credentials were provided - we'll continue.", DiagnosticMessageType.Trace);

      return password;
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
  }
}