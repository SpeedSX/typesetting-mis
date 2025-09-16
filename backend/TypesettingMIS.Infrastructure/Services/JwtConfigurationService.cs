using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Infrastructure.Services;

public class JwtConfigurationService(IConfiguration configuration) : IJwtConfigurationService
{
    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT key is not configured");

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Consistent with manual validation
        };
    }

    public string GetIssuer()
    {
        var issuer = configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("JWT issuer is not configured");
        return issuer;
    }

    public string GetAudience()
    {
        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT audience is not configured");
        return audience;
    }

    public int GetExpiryMinutes()
    {
        var raw = configuration["Jwt:ExpiryMinutes"];
        return int.TryParse(raw, out var minutes) && minutes > 0 ? minutes : 60;
    }

    public int GetRefreshTokenExpiryDays()
    {
        var raw = configuration["Jwt:RefreshTokenExpirationDays"];
        return int.TryParse(raw, out var days) && days > 0 ? days : 7;
    }

    public byte[] GetSigningKeyBytes()
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT key is not configured");
        return Encoding.UTF8.GetBytes(jwtKey);
    }
}
