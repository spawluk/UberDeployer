using System.DirectoryServices;
using NHibernate.Util;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Management.Ldap
{
  public class LdapClient : ILdapClient
  {
    private readonly string _domainControllerAddress;

    private const string _UserPrincipalNameKey = "userprincipalname";

    public LdapClient(string domainControllerAddress)
    {
      Guard.NotNullNorEmpty(domainControllerAddress, "domainControllerAddress");

      _domainControllerAddress = domainControllerAddress;
    }

    public string FindUpnUserName(string accountName)
    {
      using (var directoryEntry = new DirectoryEntry(_domainControllerAddress))
      {
        using (var directorySearcher = new DirectorySearcher(directoryEntry))
        {
          directorySearcher.Filter = string.Format("(sAMAccountName={0})", accountName);
          SearchResult searchResult = directorySearcher.FindOne();

          if (searchResult.Properties.Contains(_UserPrincipalNameKey))
          {
            return searchResult.Properties[_UserPrincipalNameKey].FirstOrNull().ToString();
          }

          return null;
        }
      }
    }
  }
}
