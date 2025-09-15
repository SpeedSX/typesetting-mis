using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Core.Services;

public interface IInvitationService
{
    Task<InvitationDto?> CreateInvitationAsync(CreateInvitationDto createInvitationDto);
    Task<InvitationDto?> ValidateInvitationAsync(string token);
    Task<bool> MarkInvitationAsUsedAsync(string token, Guid userId, string email);
}
