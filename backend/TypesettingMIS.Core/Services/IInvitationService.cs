using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Core.Services;

public interface IInvitationService
{
    Task<InvitationDto?> CreateInvitationAsync(CreateInvitationDto createInvitationDto, CancellationToken cancellationToken);
    Task<InvitationDto?> ValidateInvitationAsync(string token, CancellationToken cancellationToken);
    Task<bool> MarkInvitationAsUsedAsync(string token, Guid userId, string email, CancellationToken cancellationToken);
}
