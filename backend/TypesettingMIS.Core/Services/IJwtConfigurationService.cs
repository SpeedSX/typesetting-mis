namespace TypesettingMIS.Core.Services;

public interface IJwtConfigurationService
{
    string GetIssuer();
    string GetAudience();
    int GetExpiryMinutes();
    int GetRefreshTokenExpiryDays();
    byte[] GetSigningKeyBytes();
    string GetRefreshCookieSameSite();
    byte[] GetRefreshTokenSecretBytes();
    object GetTokenValidationParameters(); // Return object to avoid framework dependency in Core
}
