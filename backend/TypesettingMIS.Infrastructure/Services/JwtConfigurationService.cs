using Microsoft.AspNetCore.Http;
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
            ClockSkew = TimeSpan.Zero,
            
            // Additional security requirements
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
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

    public string GetRefreshCookieSameSite()
    {
        return configuration["Jwt:RefreshCookieSameSite"] ?? "Lax"; // Lax|Strict|None
    }

    public byte[] GetRefreshTokenSecretBytes()
    {
        var refreshTokenSecret = configuration["Jwt:RefreshTokenSecret"];
        if (string.IsNullOrEmpty(refreshTokenSecret))
            throw new InvalidOperationException("Refresh token secret is not configured");
        return Encoding.UTF8.GetBytes(refreshTokenSecret);
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return GetTokenValidationParameters(configuration);
    }
}
