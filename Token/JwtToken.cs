using Microsoft.IdentityModel.Tokens;
using SartainStudios.Entities.Entities;
using SartainStudios.SharedModels.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SartainStudios.Token;

public class JwtToken : IToken
{
    private static string _jwtSecret;
    private static int _jwtExpirationInMinutes;

    public JwtToken(string jwtSecret, int jwtExpirationInMinutes)
    {
        _jwtSecret = jwtSecret;
        _jwtExpirationInMinutes = jwtExpirationInMinutes;
    }

    public string GenerateToken(UserModel userModel = null) => GenerateJwtToken(_jwtSecret, _jwtExpirationInMinutes, userModel);

    public string GetUserId(string authorizationToken)
    {
        string token = GetTokenFromAuthenticationBearerString(authorizationToken);

        var claims = GetClaimsFromToken(token);

        return claims.Claims.FirstOrDefault(m => m.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
    }

    public bool IsUserLeastPrivileged(string authorizationToken)
    {
        string token = GetTokenFromAuthenticationBearerString(authorizationToken);

        var claims = GetClaimsFromToken(token);

        return claims.Claims.Any(m => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && !IsPrivilegedAccount(m.Value));
    }

    private static string GetTokenFromAuthenticationBearerString(string token) => token.Substring(7, token.Length - 7);

    private static bool IsPrivilegedAccount(string role) => role.Equals(Role.God) || role.Equals(Role.Administrator) || role.Equals(Role.Service);

    private static ClaimsPrincipal GetClaimsFromToken(string token)
    {
        string secret = _jwtSecret;

        var key = Encoding.ASCII.GetBytes(secret);
        var handler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        var claims = handler.ValidateToken(token, validations, out var tokenSecure);

        return claims;
    }

    private static string GenerateJwtToken(string jwtSecret, int jwtExpirationInMinutes, UserModel userModel = null) =>
        new JwtSecurityTokenHandler().WriteToken(CreateJwtToken(jwtSecret, jwtExpirationInMinutes, userModel));

    private static JwtSecurityToken CreateJwtToken(string jwtSecret, int jwtExpirationInMinutes, UserModel userModel = null) =>
        new(GetJwtHeader(jwtSecret), GetJwtPayload(jwtExpirationInMinutes, userModel));

    private static JwtHeader GetJwtHeader(string jwtSecret) =>
        new(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)), SecurityAlgorithms.HmacSha256));

    private static JwtPayload GetJwtPayload(int jwtExpirationInMinutes, UserModel userModel = null) =>
        new(CreateJwtClaims(jwtExpirationInMinutes, userModel));

    private static IEnumerable<Claim> CreateJwtClaims(int jwtExpirationInMinutes, UserModel userModel = null)
    {
        var userInformation = DetermineUserInformation(userModel);
        var claims = CreateClaimsArrayFromUserInformation(userInformation, jwtExpirationInMinutes);

        return claims;
    }

    private static UserModel DetermineUserInformation(UserModel userModel = null)
    {
        if (userModel == null) userModel = new UserModel();

        if (userModel.Username == null) userModel.Username = "No name found";
        if (userModel.FirstName == null) userModel.FirstName = "No first name given";
        if (userModel.LastName == null) userModel.LastName = "No last name found";
        if (userModel.Id == null) userModel.Id = "No user id found";
        if (userModel.ProfilePhoto == null) userModel.ProfilePhoto = "No profile photo found";
        if (userModel.Email == null) userModel.Email = "No email found";
        if (userModel.Roles == null) userModel.Roles = Array.Empty<string>();

        return userModel;
    }

    private static Claim[] CreateClaimsArrayFromUserInformation(UserModel userModel, int jwtExpirationInMinutes)
    {
        var claims = new List<Claim>();

        foreach (var role in userModel.Roles) claims.Add(new Claim(ClaimTypes.Role, role));

        claims.Add(new Claim(ClaimTypes.Name, userModel.Username));
        claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, userModel.FirstName + " " + userModel.LastName));
        claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userModel.Id.ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, userModel.ProfilePhoto));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, userModel.Email));
        claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddMinutes(jwtExpirationInMinutes)).ToUnixTimeSeconds().ToString()));

        return claims.ToArray();
    }
}