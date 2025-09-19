using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Application.Features.Auth.Commands.Login;
using TypesettingMIS.Application.Features.Auth.Commands.Logout;
using TypesettingMIS.Application.Features.Auth.Commands.RefreshToken;
using TypesettingMIS.Application.Features.Auth.Commands.Register;
using TypesettingMIS.Application.Features.Auth.Queries.GetCurrentUser;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, IWebHostEnvironment environment, IJwtConfigurationService jwtConfiguration)
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
        var command = new LoginCommand(loginDto.Email, loginDto.Password);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        var cookieOptions = BuildRefreshCookieOptions();
        Response.Cookies.Append(RefreshCookieName, result.Data!.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.Data.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result.Data);
    }

    /// <summary>
    /// User registration - creates new user account, sets httpOnly refresh token cookie
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            registerDto.Email,
            registerDto.Password,
            registerDto.FirstName,
            registerDto.LastName,
            registerDto.InvitationToken);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        // Set httpOnly refresh token cookie
        var cookieOptions = BuildRefreshCookieOptions();
        Response.Cookies.Append(RefreshCookieName, result.Data!.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.Data.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result.Data);
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

        var command = new RefreshTokenCommand(refreshToken);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        var cookieOptions = BuildRefreshCookieOptions();
        Response.Cookies.Append(RefreshCookieName, result.Data!.RefreshToken, cookieOptions);

        // Remove refresh token from response body for security
        result.Data.RefreshToken = string.Empty;

        SetNoStoreHeaders();

        return Ok(result.Data);
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
            var command = new LogoutCommand(refreshToken);
            await mediator.Send(command, cancellationToken);
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
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
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

        var query = new GetCurrentUserQuery(
            userId, userEmail, userName, given, surname, 
            companyId, roleId, roleName, isActive);

        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized();
        }

        return Ok(result.Data);
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