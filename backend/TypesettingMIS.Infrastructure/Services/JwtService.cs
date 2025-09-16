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
        var key = jwtConfig.GetSigningKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("company_id", user.CompanyId.ToString()),
            new("role_id", user.RoleId.ToString()),
            new("role_name", user.Role?.Name ?? ""),
            new("is_active", user.IsActive.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Role?.Name))
            claimsList.Add(new Claim(ClaimTypes.Role, user.Role!.Name));

        var claims = claimsList.ToArray();

        var token = new JwtSecurityToken(
            issuer: jwtConfig.GetIssuer(),
            audience: jwtConfig.GetAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtConfig.GetExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, jwtConfig.GetTokenValidationParameters(), out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
