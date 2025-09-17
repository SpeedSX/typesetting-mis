using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IWebHostEnvironment environment, IJwtConfigurationService jwtConfiguration)
    : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";

    /// <summary>
    /// User login - returns JWT token and user information, sets httpOnly refresh token cookie
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(loginDto, cancellationToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var cookieOptions = BuildRefreshCookieOptions();

        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result);
    }

    /// <summary>
    /// User registration - creates new user account, sets httpOnly refresh token cookie
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(registerDto, cancellationToken);

        if (result == null)
        {
            return BadRequest(new { message = "Registration failed. User may already exist or company not found." });
        }

        // Set httpOnly refresh token cookie
        var cookieOptions = BuildRefreshCookieOptions();

        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result);
    }

    /// <summary>
    /// Refresh JWT token using refresh token from httpOnly cookie
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(CancellationToken cancellationToken)
    {
        // Read refresh token from httpOnly cookie
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) ||
            string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "No refresh token found in cookie" });
        }

        var result = await authService.RefreshTokenAsync(refreshToken, cancellationToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var cookieOptions = BuildRefreshCookieOptions();

        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result);
    }

    /// <summary>
    /// User logout - invalidates refresh token and clears cookie
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Read refresh token from httpOnly cookie
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
        {
            await authService.LogoutAsync(refreshToken, cancellationToken);
        }

        // Clear the refresh token cookie
        Response.Cookies.Delete(RefreshCookieName, BuildRefreshCookieOptions(forDeletion: true));

        return NoContent();
    }


    /// <summary>
    /// Get current user information from JWT token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserDto> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var given = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
        var surname = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
        var companyId = User.FindFirst("company_id")?.Value;
        var roleId = User.FindFirst("role_id")?.Value;
        var roleName = User.FindFirst("role_name")?.Value;
        var isActive = User.FindFirst("is_active")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var parts = userName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var first = given ?? (parts.Length > 0 ? parts[0] : "");
        var last = surname ?? (parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "");

        var user = new UserDto
        {
            Id = parsedUserId,
            Email = userEmail ?? "",
            FirstName = first,
            LastName = last,
            CompanyId = Guid.TryParse(companyId, out var cid) ? cid : Guid.Empty,
            RoleId = Guid.TryParse(roleId, out var rid) ? rid : Guid.Empty,
            RoleName = roleName ?? "",
            IsActive = bool.TryParse(isActive, out var active) && active
        };

        return Ok(user);
    }

    private CookieOptions BuildRefreshCookieOptions(bool forDeletion = false)
    {
        var sameSiteStr = jwtConfiguration.GetRefreshCookieSameSite(); // Lax|Strict|None
        var sameSite = SameSiteMode.Lax;
        if (Enum.TryParse<SameSiteMode>(sameSiteStr, true, out var parsed)) sameSite = parsed;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // SameSite=None requires Secure=true; override even in Development to avoid silent drops.
            Secure = sameSite == SameSiteMode.None || !environment.IsDevelopment(),
            SameSite = sameSite,
            Path = "/api"
        };

        if (!forDeletion)
        {
            var days = jwtConfiguration.GetRefreshTokenExpiryDays();
            cookieOptions.MaxAge = TimeSpan.FromDays(days);
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(days);
        }

        return cookieOptions;
    }

    private void SetNoStoreHeaders()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        Response.Headers["Vary"] = "Authorization";
    }
}