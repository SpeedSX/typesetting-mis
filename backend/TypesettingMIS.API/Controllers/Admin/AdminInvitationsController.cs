using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers.Admin;

[Route("api/admin/invitations")]
[Authorize(Roles = "Admin")]
public class AdminInvitationsController(IInvitationService invitationService, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Create a new invitation for a company
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InvitationDto>> CreateInvitation(CreateInvitationDto createInvitationDto, CancellationToken cancellationToken)
    {
        var invitation = await invitationService.CreateInvitationAsync(createInvitationDto, cancellationToken);

        if (invitation == null)
        {
            return BadRequest(new { message = "Company not found or invitation creation failed" });
        }

        return Ok(invitation);
    }

    /// <summary>
    /// Validate an invitation token
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationDto>> ValidateInvitation(ValidateInvitationDto validateInvitationDto, CancellationToken cancellationToken)
    {
        var invitation = await invitationService.ValidateInvitationAsync(validateInvitationDto.Token, cancellationToken);

        if (invitation == null)
        {
            return BadRequest(new { message = "Invalid or expired invitation token" });
        }

        return Ok(invitation);
    }
}
