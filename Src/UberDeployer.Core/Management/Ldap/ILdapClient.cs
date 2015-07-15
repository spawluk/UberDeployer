namespace UberDeployer.Core.Management.Ldap
{
  public interface ILdapClient
  {
    string FindUpnUserName(string accountName);
  }
}