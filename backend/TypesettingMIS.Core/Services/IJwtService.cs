using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Core.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}
