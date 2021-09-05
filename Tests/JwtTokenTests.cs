using NUnit.Framework;
using Sartain_Studios_Common.SharedEntities;
using SartainStudios.Token;
using SharedModels;

namespace Tests;

public class JwtTokenTests
{
    private IToken _token;

    private readonly string jwtSecret = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";

    private readonly int jwtExpirationInMinutes = 60;

    [SetUp]
    public void Setup()
    {
        _token = new JwtToken(jwtSecret, jwtExpirationInMinutes);
    }

    #region GenerateToken
    [Test]
    public void GenerateToken_ReturnsToken()
    {
        var result = _token.GenerateToken();

        Assert.GreaterOrEqual(result.Length, 50);
    }

    [Test]
    public void GenerateToken_WithNullUserModelReturnsToken()
    {
        var result = _token.GenerateToken(null);

        Assert.GreaterOrEqual(result.Length, 50);
    }

    [Test]
    public void GenerateToken_WithEmptyUserModelReturnsToken()
    {
        var result = _token.GenerateToken(new UserModel { });

        Assert.GreaterOrEqual(result.Length, 50);
    }
    #endregion

    #region IsUserLeastPrivileged
    [Test]
    public void IsUserLeastPrivileged_ReturnsFalseIfUserIsNotLeastPrivileged()
    {
        var token = _token.GenerateToken(new UserModel { Roles = new string[] { Role.Service } });

        var authorizationString = "Bearer " + token;

        var result = _token.IsUserLeastPrivileged(authorizationString);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsUserLeastPrivileged_ReturnsTrueIfUserIsLeastPrivileged()
    {
        var token = _token.GenerateToken(new UserModel { Roles = new string[] { Role.User } });

        var authorizationString = "Bearer " + token;

        var result = _token.IsUserLeastPrivileged(authorizationString);

        Assert.IsTrue(result);
    }
    #endregion

    #region
    [Test]
    public void GetUserId_Returns_NoUserId_If_No_Id_Exists()
    {
        var token = _token.GenerateToken(new UserModel());

        var authorizationString = "Bearer " + token;

        var result = _token.GetUserId(authorizationString);

        Assert.AreEqual("No user id found", result);
    }

    [Test]
    public void GetUserId_Returns_UserId_If_Id_Exists()
    {
        var userId = "609caf1721398f1d23111b0f";

        var token = _token.GenerateToken(new UserModel { Id = userId });

        var authorizationString = "Bearer " + token;

        var result = _token.GetUserId(authorizationString);

        Assert.AreEqual(userId, result);
    }
    #endregion
}