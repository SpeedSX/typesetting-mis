using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.Infrastructure.Services;

public class AuthService(
    ApplicationDbContext context,
    IJwtService jwtService,
    IPasswordHasher<User> passwordHasher,
    IInvitationService invitationService,
    IHttpContextAccessor httpContextAccessor,
    IJwtConfigurationService jwtConfigurationService)
    : IAuthService
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

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var normalized = loginDto.Email.Trim().ToUpperInvariant();
        var user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user is not { IsActive: true, PasswordHash: not null, Email: not null })
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (result != PasswordVerificationResult.Success)
            return null;

        user.LastLogin = DateTime.UtcNow;
        var token = jwtService.GenerateToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token hash in database
        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(refreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false,
            //CreatedByIp = httpContext?.Connection?.RemoteIpAddress?.ToString()
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
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
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? "",
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "",
                IsActive = user.IsActive
            }
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        // Validate invitation token
        var invitation = await invitationService.ValidateInvitationAsync(registerDto.InvitationToken, cancellationToken);
        if (invitation == null)
            return null;

        // Get company from invitation
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId && !c.IsDeleted, cancellationToken);

        if (company == null)
            return null;

        // Check if user already exists
        var email = registerDto.Email.Trim();
        var normalized = email.ToUpperInvariant();
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.CompanyId == invitation.CompanyId && u.NormalizedEmail == normalized, cancellationToken);

        if (existingUser != null)
            return null;

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
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CompanyId = company.Id,
            RoleId = defaultRole.Id,
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, registerDto.Password);

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
        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(refreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        // Mark invitation as used
        await invitationService.MarkInvitationAsUsedAsync(registerDto.InvitationToken, user.Id, user.Email!, cancellationToken);

        return new AuthResponseDto
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
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? "",
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "",
                IsActive = user.IsActive
            }
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // Compute hash of the incoming refresh token
        var tokenHash = ComputeRefreshTokenHash(refreshToken);

        // Find the refresh token hash in database
        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Company)
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return null; // Invalid or expired token
        }

        // Get the user
        var user = storedToken.User;
        if (user is not { IsActive: true, Email: not null })
        {
            return null; // User not found or inactive
        }

        // Generate new tokens
        var newToken = jwtService.GenerateToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        storedToken.ReplacedByTokenHash = ComputeRefreshTokenHash(newRefreshToken);

        // Store new refresh token hash
        var newRefreshTokenEntity = new RefreshToken
        {
            TokenHash = ComputeRefreshTokenHash(newRefreshToken),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtConfigurationService.GetRefreshTokenExpiryDays()),
            IsRevoked = false
        };
        context.RefreshTokens.Add(newRefreshTokenEntity);

        await context.SaveChangesAsync(cancellationToken); // expect concurrency handling at DB level
        await tx.CommitAsync(cancellationToken);

        return new AuthResponseDto
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
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? "",
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "",
                IsActive = user.IsActive
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // Compute hash of the incoming refresh token
        var tokenHash = ComputeRefreshTokenHash(refreshToken);

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

        return true;
    }
}
