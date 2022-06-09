using SartainStudios.SharedModels.User;

namespace SartainStudios.Token;

public interface IToken
{
    string GenerateToken(UserModel userModel = null);
    string GetUserId(string authorizationToken);
    bool IsUserLeastPrivileged(string authorizationToken);
}