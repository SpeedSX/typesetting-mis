using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Core.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}
