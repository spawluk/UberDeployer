using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  }
}
