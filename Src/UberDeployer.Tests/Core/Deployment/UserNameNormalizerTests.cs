using System;
using NUnit.Framework;
using UberDeployer.Core.Deployment;

namespace UberDeployer.Tests.Core.Deployment
{
  [TestFixture]
  public class UserNameNormalizerTests
  {
    private UserNameNormalizer _userNameNormalizer;

    [SetUp]
    public void SetUp()
    {
      _userNameNormalizer = new UserNameNormalizer();
    }

    [TestCase("userName@domain")]
    [TestCase("user.name@domain")]
    [TestCase("userName@domain.name")]
    [TestCase("userName@domain.name.com")]
    [TestCase("user_name@domain")]
    [TestCase("user-name@domain")]
    public void IsUpnUserName_properly_checks_user_name_format(string userName)
    {
      // act
      bool isUpnUserName = _userNameNormalizer.IsUpnUserName(userName);

      // assert
      Assert.IsTrue(isUpnUserName);
    }

    [TestCase(@"domainName\userName")]
    [TestCase(@"onlyUserName")]
    [TestCase(@"user@name@domain")]
    public void IsUpnUserName_returns_false_when_user_name_is_invalid(string invalidUserName)
    {
      // act
      bool isUpnUserName = _userNameNormalizer.IsUpnUserName(invalidUserName);

      // assert
      Assert.IsFalse(isUpnUserName);
    }

    [Test]
    public void ConvertToPreWin2000UserName_converts_from_Upn_format_to_pre_windows2000()
    {
      // arrange 
      const string expectedPreWin2000UserName = "domain-name\\username";
      const string upnUserName = "username@domain.name";
      const string domainName = "domain-name";

      // act
      string preWin2000UserName = _userNameNormalizer.ConvertToPreWin2000UserName(upnUserName, domainName);

      // assert
      Assert.AreEqual(expectedPreWin2000UserName, preWin2000UserName);
    }

    [Test]
    public void ConvertToPreWin2000UserName_returns_the_same_value_when_given_user_name_is_already_in_this_format()
    {
      // arrange  
      const string expectedPreWin2000UserName = "domain-name\\username";
      const string domainName = "domain-name";

      // act
      string preWin2000UserName = _userNameNormalizer.ConvertToPreWin2000UserName(expectedPreWin2000UserName, domainName);

      // assert
      Assert.AreEqual(expectedPreWin2000UserName, preWin2000UserName);
    }

    [Test]
    public void ConvertToPreWin2000UserName_fails_when_given_user_name_is_invalid()
    {
      // arrange  
      const string domainName = "domain-name";
      const string invalidUserName = "invalid#User$";

      // act assert
      Assert.Throws<InvalidOperationException>(
        () => _userNameNormalizer.ConvertToPreWin2000UserName(invalidUserName, domainName));
    }
  }
}
