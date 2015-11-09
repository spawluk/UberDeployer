using System;
using System.Management.Automation;

namespace UberDeployer.Core.Management.PowerShell
{
  public class PowerShellScriptExecutionException : Exception
  {
    public PSDataCollection<ErrorRecord> Errors { get; private set; }

    public PowerShellScriptExecutionException(PSDataCollection<ErrorRecord> errors)
    {
      Errors = errors;
    }
  }
}
