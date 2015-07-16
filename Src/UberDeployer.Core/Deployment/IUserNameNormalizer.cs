namespace UberDeployer.Core.Deployment
{
  public interface IUserNameNormalizer
  {
    bool IsUpnUserName(string userName);

    bool IsPreWin2000UserName(string userName);

    string ConvertToPreWin2000UserName(string userName, string domainName);
  }
}