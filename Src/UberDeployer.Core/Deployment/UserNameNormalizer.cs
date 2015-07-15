using System.Text.RegularExpressions;

namespace UberDeployer.Core.Deployment
{
  public class UserNameNormalizer
  {
    private const string _PreWin2000UserNamePattern = @"[\w\.-_]+\\[\w-_]+";

    private const string _UpnUserNamePattern = @"[\w\.-_]+@([\w.]+)?\w+";

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
  }
}
