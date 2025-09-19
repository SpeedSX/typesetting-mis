using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Core.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
