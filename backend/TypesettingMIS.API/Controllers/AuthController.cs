using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// User login - returns JWT token and user information
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
    }

    /// <summary>
    /// User registration - creates new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        
        if (result == null)
        {
            return BadRequest(new { message = "Registration failed. User may already exist or company not found." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        return Ok(result);
    }

    /// <summary>
    /// User logout - invalidates refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _authService.LogoutAsync(refreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user information from JWT token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var companyId = User.FindFirst("company_id")?.Value;
        var roleId = User.FindFirst("role_id")?.Value;
        var roleName = User.FindFirst("role_name")?.Value;
        var isActive = User.FindFirst("is_active")?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var user = new UserDto
        {
            Id = Guid.Parse(userId),
            Email = userEmail ?? "",
            FirstName = userName?.Split(' ').FirstOrDefault() ?? "",
            LastName = userName?.Split(' ').Skip(1).FirstOrDefault() ?? "",
            CompanyId = Guid.Parse(companyId ?? Guid.Empty.ToString()),
            RoleId = Guid.Parse(roleId ?? Guid.Empty.ToString()),
            RoleName = roleName ?? "",
            IsActive = bool.Parse(isActive ?? "false")
        };

        return Ok(user);
    }
}
