using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext context,
    IJwtService jwtService,
    IPasswordHasher<User> passwordHasher,
    IJwtConfigurationService jwtConfigurationService)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
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

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user is not { IsActive: true, PasswordHash: not null, Email: not null } ||
            user.Company is { IsDeleted: true })
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        }

        user.LastLogin = DateTime.UtcNow;
        var token = jwtService.GenerateToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token hash in database
        var refreshTokenEntity = new Core.Entities.RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(refreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false,
            //CreatedByIp = httpContext?.Connection?.RemoteIpAddress?.ToString()
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = jwtService.GetExpiryUtc(token),
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