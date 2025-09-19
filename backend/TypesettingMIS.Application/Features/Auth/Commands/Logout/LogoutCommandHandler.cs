using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IJwtConfigurationService jwtConfigurationService)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <summary>
    /// Computes HMAC-SHA256 hash of the refresh token using the configured secret
    /// </summary>
    private string ComputeRefreshTokenHash(string token)
    {
        var secret = jwtConfigurationService.GetRefreshTokenSecretBytes();
        using var hmac = new HMACSHA256(secret);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Compute hash of the incoming refresh token
        var tokenHash = ComputeRefreshTokenHash(request.RefreshToken);

        // Find and revoke the refresh token
        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReasonRevoked = "User logout";
            storedToken.RevokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}