using System;

namespace UberDeployer.Core.Management.PowerShell
{
  public class PowerShellConfiguration
  {
    public bool IsRemote { get; set; }
    
    public string RemoteMachineName { get; set; }

    public Action<string> OnOutput { get; set; }

    public Action<string> OnError { get; set; }
  }
}
