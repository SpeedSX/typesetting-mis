using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers.Admin;

[Route("api/admin/invitations")]
[Authorize(Roles = "Admin")]
public class AdminInvitationsController : BaseController
{
    private readonly IInvitationService _invitationService;

    public AdminInvitationsController(IInvitationService invitationService, ITenantContext tenantContext) 
        : base(tenantContext)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Create a new invitation for a company
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InvitationDto>> CreateInvitation(CreateInvitationDto createInvitationDto)
    {
        var invitation = await _invitationService.CreateInvitationAsync(createInvitationDto);

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
    public async Task<ActionResult<InvitationDto>> ValidateInvitation(ValidateInvitationDto validateInvitationDto)
    {
        var invitation = await _invitationService.ValidateInvitationAsync(validateInvitationDto.Token);

        if (invitation == null)
        {
            return BadRequest(new { message = "Invalid or expired invitation token" });
        }

        return Ok(invitation);
    }
}
