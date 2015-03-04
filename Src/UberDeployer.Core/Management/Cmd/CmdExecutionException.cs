using System;

namespace UberDeployer.Core.Management.Cmd
{
  public class CmdExecutionException : Exception
  {
    public CmdExecutionException(string executedFile, string arguments, int errorCode, string message)
      : base(string.Format(
        "Error on executing command line, executed file: '{1}' with arguments: '{2}', error code: [{0}], error message: {3}",
        executedFile, 
        arguments, 
        errorCode, 
        message))
    {      
    }
  }
}
