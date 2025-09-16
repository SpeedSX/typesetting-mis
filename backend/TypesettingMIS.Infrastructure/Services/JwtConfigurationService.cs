using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Infrastructure.Services;

public class JwtConfigurationService(IConfiguration configuration) : IJwtConfigurationService
{
    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero // Consistent with manual validation
        };
    }

    public TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = GetIssuer(),
            ValidAudience = GetAudience(),
            IssuerSigningKey = GetSigningKey(),
            ClockSkew = TimeSpan.Zero // Consistent with manual validation
        };
    }

    public SymmetricSecurityKey GetSigningKey()
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT key is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        return key;
    }

    public string GetIssuer()
    {
        return configuration["Jwt:Issuer"]!;
    }

    public string GetAudience()
    {
        return configuration["Jwt:Audience"]!;
    }

    public int GetExpiryMinutes()
    {
        return int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");
    }
}
