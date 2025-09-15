using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IInvitationService _invitationService;

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IPasswordHasher<User> passwordHasher,
        IInvitationService invitationService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _invitationService = invitationService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !user.IsActive)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (result != PasswordVerificationResult.Success)
            return null;

        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

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
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

        if (existingUser != null)
            return null;

        // Validate invitation token
        var invitation = await _invitationService.ValidateInvitationAsync(registerDto.InvitationToken);
        if (invitation == null)
            return null;

        // Get company from invitation
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == invitation.CompanyName && !c.IsDeleted);

        if (company == null)
            return null;

        // Get default role (or create one)
        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.CompanyId == company.Id && r.Name == "User");

        if (defaultRole == null)
        {
            defaultRole = new Role
            {
                Name = "User",
                CompanyId = company.Id,
                Permissions = "[]" // Basic permissions
            };
            _context.Roles.Add(defaultRole);
            await _context.SaveChangesAsync();
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

        user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Load the user with related data
        user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id);

        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Mark invitation as used
        await _invitationService.MarkInvitationAsUsedAsync(registerDto.InvitationToken, user.Id, user.Email);

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
        var storedToken = await _context.RefreshTokens
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
        if (user == null || !user.IsActive)
        {
            return null; // User not found or inactive
        }

        // Generate new tokens
        var newToken = _jwtService.GenerateToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken;

        // Store new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _context.SaveChangesAsync();

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
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReasonRevoked = "User logout";
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
