using NUnit.Framework;
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
}