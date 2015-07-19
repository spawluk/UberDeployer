using System;
using System.Collections.Generic;
using UberDeployer.Agent.Service;
using UberDeployer.Common;
using UberDeployer.CommonConfiguration;

namespace UberDeployer.Agent.NtService
{
  public class UberDeployerAgentServiceHostContainer : MyServiceHostContainer
  {
    protected override void OnServiceHostsStarting()
    {
      bool mockTeamCity = AppSettingsUtils.ReadAppSettingBool("MockTeamCity");
      Bootstraper.Bootstrap(mockTeamCity);

      base.OnServiceHostsStarting();
    }

    protected override IEnumerable<Type> ServiceTypes
    {
      get { yield return typeof(AgentService); }
    }
  }
}
