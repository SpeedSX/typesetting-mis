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
            new("role_name", user.Role?.Name ?? ""),
            new("is_active", user.IsActive.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)
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
        return Base64UrlEncoder.Encode(bytes);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var validationParameters = (TokenValidationParameters)jwtConfig.GetTokenValidationParameters();
            // Add additional validation requirements specific to manual validation
            validationParameters.RequireSignedTokens = true;
            validationParameters.RequireExpirationTime = true;
            validationParameters.ValidAlgorithms = [SecurityAlgorithms.HmacSha256];
            
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken _);
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
            if (!ValidateToken(token))
                return null;
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public DateTime GetExpiryUtc(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
    }
}
