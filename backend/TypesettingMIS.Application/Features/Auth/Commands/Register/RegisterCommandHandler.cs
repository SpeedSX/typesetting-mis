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

namespace TypesettingMIS.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IApplicationDbContext context,
    IJwtService jwtService,
    IPasswordHasher<User> passwordHasher,
    IInvitationService invitationService,
    IJwtConfigurationService jwtConfigurationService)
    : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
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

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validate invitation token
        var invitation = await invitationService.ValidateInvitationAsync(request.InvitationToken, cancellationToken);
        if (invitation == null)
            return Result<AuthResponseDto>.Failure("Invalid or expired invitation token");

        // Get company from invitation
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId && !c.IsDeleted, cancellationToken);

        if (company == null)
            return Result<AuthResponseDto>.Failure("Company not found or no longer available");

        // Check if user already exists
        var email = request.Email.Trim();
        var normalized = email.ToUpperInvariant();
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.CompanyId == invitation.CompanyId && u.NormalizedEmail == normalized, cancellationToken);

        if (existingUser != null)
            return Result<AuthResponseDto>.Failure("User with this email already exists in this company");

        // Get default role (or create one)
        var defaultRole = await context.Roles
            .FirstOrDefaultAsync(r => r.CompanyId == company.Id && r.Name == "User", cancellationToken);

        if (defaultRole == null)
        {
            defaultRole = new Role
            {
                Name = "User",
                NormalizedName = "USER",
                CompanyId = company.Id,
                Permissions = "[]" // Basic permissions
            };
            context.Roles.Add(defaultRole);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Create new user
        var user = new User
        {
            Email = email,
            NormalizedEmail = normalized,
            UserName = email,
            NormalizedUserName = normalized,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyId = company.Id,
            RoleId = defaultRole.Id,
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        // Load the user with related data
        user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        var token = jwtService.GenerateToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token hash in database
        var refreshTokenEntity = new Core.Entities.RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(refreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        // Mark invitation as used
        await invitationService.MarkInvitationAsUsedAsync(request.InvitationToken, user.Id, user.Email!, cancellationToken);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = jwtService.GetExpiryUtc(token),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
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