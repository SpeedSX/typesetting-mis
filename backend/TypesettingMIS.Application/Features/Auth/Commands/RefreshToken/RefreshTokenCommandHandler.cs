using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IJwtService jwtService,
    IHttpContextAccessor httpContextAccessor,
    IJwtConfigurationService jwtConfigurationService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
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

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Compute hash of the incoming refresh token
        var tokenHash = ComputeRefreshTokenHash(request.RefreshToken);

        // Find the refresh token hash in database
        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Company)
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token");
        }

        // Get the user
        var user = storedToken.User;
        if (user is not { IsActive: true, Email: not null })
        {
            return Result<AuthResponseDto>.Failure("User not found or inactive");
        }

        // Generate new tokens
        var newToken = jwtService.GenerateToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        storedToken.ReplacedByTokenHash = ComputeRefreshTokenHash(newRefreshToken);

        // Store new refresh token hash
        var newRefreshTokenEntity = new Core.Entities.RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(newRefreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false
        };
        context.RefreshTokens.Add(newRefreshTokenEntity);

        await context.SaveChangesAsync(cancellationToken);

        var authResponse = new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = jwtService.GetExpiryUtc(newToken),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId.GetValueOrDefault(),
                CompanyName = user.Company?.Name ?? "",
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "",
                IsActive = user.IsActive
            }
        };

        return Result<AuthResponseDto>.Success(authResponse);
    }
}