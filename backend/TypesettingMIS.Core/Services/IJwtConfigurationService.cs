namespace TypesettingMIS.Core.Services;

public interface IJwtConfigurationService
{
    string GetIssuer();
    string GetAudience();
    int GetExpiryMinutes();
    byte[] GetSigningKeyBytes();
}
