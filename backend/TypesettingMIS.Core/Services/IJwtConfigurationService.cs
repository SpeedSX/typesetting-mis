using Microsoft.IdentityModel.Tokens;

namespace TypesettingMIS.Core.Services;

public interface IJwtConfigurationService
{
    TokenValidationParameters GetTokenValidationParameters();
    SymmetricSecurityKey GetSigningKey();
    string GetIssuer();
    string GetAudience();
    int GetExpiryMinutes();
}
