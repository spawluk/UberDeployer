using System;
using System.Text.RegularExpressions;

namespace UberDeployer.Core.Deployment
{
  public class UserNameNormalizer : IUserNameNormalizer
  {
    private const string _PreWin2000UserNamePattern = @"^[\w\.\-_]+\\[\w\-_]+$";

    private const string _UpnUserNamePattern = @"^[\w\.\-_]+@([\w.]+)?\w+$";

    public bool IsUpnUserName(string userName)
    {
      var regex = new Regex(_UpnUserNamePattern);

      return regex.IsMatch(userName);
    }

    public bool IsPreWin2000UserName(string userName)
    {      
      var regex = new Regex(_PreWin2000UserNamePattern);

      return regex.IsMatch(userName);
    }

    public string ConvertToPreWin2000UserName(string userName, string domainName)
    {
      if (IsPreWin2000UserName(userName))
      {
        return userName;
      }

      if (IsUpnUserName(userName))
      {
        string accountName = GetAccountName(userName);

        return string.Format("{0}\\{1}", domainName, accountName);
      }

      throw new InvalidOperationException("Given user name is neither in UPN or pre-windows 2000 format.");
    }

    private static string GetAccountName(string userName)
    {
      return userName.Substring(0, userName.IndexOf('@'));
    }
  }
}
