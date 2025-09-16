using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    IConfiguration configuration)
    : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user is not { IsActive: true, PasswordHash: not null, Email: not null })
            return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (result != PasswordVerificationResult.Success)
            return null;

        var token = jwtService.GenerateToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7)),
            IsRevoked = false
            // CreatedByIp = httpContext?.Connection?.RemoteIpAddress?.ToString()
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiry
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
                IsActive = user.IsActive,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

        if (existingUser != null)
            return null;

        // Validate invitation token
        var invitation = await invitationService.ValidateInvitationAsync(registerDto.InvitationToken);
        if (invitation == null)
            return null;

        // Get company from invitation
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == invitation.CompanyId && !c.IsDeleted);

        if (company == null)
            return null;

        // Get default role (or create one)
        var defaultRole = await context.Roles
            .FirstOrDefaultAsync(r => r.CompanyId == company.Id && r.Name == "User");

        if (defaultRole == null)
        {
            defaultRole = new Role
            {
                Name = "User",
                CompanyId = company.Id,
                Permissions = "[]" // Basic permissions
            };
            context.Roles.Add(defaultRole);
            await context.SaveChangesAsync();
        }

        // Create new user
        var user = new User
        {
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CompanyId = company.Id,
            RoleId = defaultRole.Id,
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, registerDto.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Load the user with related data
        user = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id);

        var token = jwtService.GenerateToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7)),
            IsRevoked = false
        };
        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync();

        // Mark invitation as used
        await invitationService.MarkInvitationAsUsedAsync(registerDto.InvitationToken, user.Id, user.Email);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
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
                IsActive = user.IsActive,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token in database
        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Company)
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

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

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        storedToken.ReplacedByToken = newRefreshToken;

        // Store new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7)),
            IsRevoked = false
        };
        context.RefreshTokens.Add(newRefreshTokenEntity);

        await context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
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
                IsActive = user.IsActive,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        // Find and revoke the refresh token
        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReasonRevoked = "User logout";
            await context.SaveChangesAsync();
        }

        return true;
    }
}
