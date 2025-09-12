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

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
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

        // Store refresh token (in a real app, you'd store this in a separate table)
        // For now, we'll just return it

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

        // Check if company exists
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == registerDto.CompanyId && !c.IsDeleted);

        if (company == null)
            return null;

        // Get default role (or create one)
        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.CompanyId == registerDto.CompanyId && r.Name == "User");

        if (defaultRole == null)
        {
            defaultRole = new Role
            {
                Name = "User",
                CompanyId = registerDto.CompanyId,
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
            CompanyId = registerDto.CompanyId,
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
        // In a real app, you'd validate the refresh token against a stored value
        // For now, we'll just generate a new token (this is not secure!)
        // TODO: Implement proper refresh token validation
        
        return null; // Not implemented yet
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        // In a real app, you'd invalidate the refresh token
        // For now, we'll just return true
        return await Task.FromResult(true);
    }
}
