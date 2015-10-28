using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Microsoft.AspNet.SignalR;
using UberDeployer.Agent.Proxy;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.WebApp.Core.Connectivity
{
  [Authorize]
  public class DiagnosticMessagesHub : Hub
  {
    private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly IDictionary<string, string> _connectionIdUserIdentityDict = new Dictionary<string, string>();
    private static readonly IDictionary<string, Timer> _userTimers = new Dictionary<string, Timer>(); 

    private readonly IAgentService _agentService;

    private string UserIdentity
    {
      get
      {
        string userIdentity = Context.User.With(x => x.Identity).With(x => x.Name);

        if (string.IsNullOrEmpty(userIdentity))
        {
          throw new InvalidOperationException("No user identity!");
        }

        return userIdentity;
      }
    }

    public DiagnosticMessagesHub()
      : this(new AgentServiceClient())
    {
    }

    public DiagnosticMessagesHub(IAgentService agentService)
    {
      _agentService = agentService;
    }

    public override Task OnConnected()
    {
      _log.DebugFormat("Client [{0}] connected", UserIdentity);

      _connectionIdUserIdentityDict[UserIdentity] = Context.ConnectionId;
      
      var timer = new Timer(500);

      timer.AutoReset = true;
      timer.Enabled = true;
      timer.Elapsed += (sender, args) => OnDiagnosticMessage();
      timer.Start();

      _userTimers.Add(UserIdentity, timer);

      return base.OnConnected();
    }

    public override Task OnDisconnected()
    {
      _log.DebugFormat("Client [{0}] disconnected", UserIdentity);

      string userIdentity = UserIdentity;

      _connectionIdUserIdentityDict.Remove(userIdentity);

      var timer = _userTimers[UserIdentity];

      _userTimers.Remove(UserIdentity);

      timer.Stop();
      timer.Dispose();

      return base.OnDisconnected();
    }

    public void OnDiagnosticMessage()
    {
      var client = GetClient(UserIdentity);

      client.OnDiagnosticMessage("Message");
    }

    private static dynamic GetClient(string userIdentity)
    {
      Guard.NotNullNorEmpty(userIdentity, "userIdentity");

      string connectionId;

      if (!_connectionIdUserIdentityDict.TryGetValue(userIdentity, out connectionId))
      {
        throw new ClientNotConnectedException(userIdentity);
      }

      IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<DeploymentHub>();

      dynamic client = hubContext.Clients.Client(connectionId);

      if (client == null)
      {
        throw new ClientNotConnectedException(userIdentity);
      }

      return client;
    }
  }
}