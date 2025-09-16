using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers.User;

[ApiController]
[Route("api/user/invitations")]
public class UserInvitationsController(IInvitationService invitationService, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Validate an invitation token
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationDto>> ValidateInvitation(ValidateInvitationDto validateInvitationDto,
        CancellationToken cancellationToken)
    {
        var invitation =
            await invitationService.ValidateInvitationAsync(validateInvitationDto.Token, cancellationToken);

        if (invitation == null)
        {
            return BadRequest(new { message = "Invalid or expired invitation token" });
        }

        return Ok(invitation);
    }
}