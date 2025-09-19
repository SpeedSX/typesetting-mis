using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Infrastructure.Services;

public class JwtService(IJwtConfigurationService jwtConfig) : IJwtService
{
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(jwtConfig.GetSigningKeyBytes());
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var notBefore = now.AddSeconds(-30); // Set NBF 30 seconds in the past to avoid timing issues
        
        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("company_id", user.CompanyId.ToString()),
            new("role_id", user.RoleId.ToString()),
            new("is_active", user.IsActive ? "true" : "false", ClaimValueTypes.Boolean),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(notBefore).ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrWhiteSpace(user.Role?.Name))
            claimsList.Add(new Claim(ClaimTypes.Role, user.Role!.Name));

        var claims = claimsList.ToArray();

        var token = new JwtSecurityToken(
            issuer: jwtConfig.GetIssuer(),
            audience: jwtConfig.GetAudience(),
            claims: claims,
            notBefore: notBefore, // Explicitly set notBefore
            expires: now.AddMinutes(jwtConfig.GetExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncoder.Encode(bytes);
    }

    public DateTime GetExpiryUtc(string token)
    {
        var dt = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}
