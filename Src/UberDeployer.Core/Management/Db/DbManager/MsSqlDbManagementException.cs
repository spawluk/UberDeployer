using System;

namespace UberDeployer.Core.Management.Db.DbManager
{
  public class MsSqlDbManagementException : Exception
  {
    public MsSqlDbManagementException(string description)
      :this(description, null)
    {
    }

    public MsSqlDbManagementException(string description, Exception innerException)
      :base(description, innerException)
    {
    }
  }
}
