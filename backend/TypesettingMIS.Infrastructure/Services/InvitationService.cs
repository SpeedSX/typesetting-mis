using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TypesettingMIS.Core.DTOs.Auth;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.Infrastructure.Services;

public class InvitationService(ApplicationDbContext context) : IInvitationService
{
    public async Task<InvitationDto?> CreateInvitationAsync(CreateInvitationDto createInvitationDto, CancellationToken cancellationToken)
    {
        // Verify company exists
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == createInvitationDto.CompanyId && !c.IsDeleted, cancellationToken);

        if (company == null)
        {
            return null;
        }

        // Generate secure token
        var token = GenerateSecureToken();

        // Create invitation
        var invitation = new Invitation
        {
            Token = token,
            CompanyId = createInvitationDto.CompanyId,
            ExpiresAt = DateTime.UtcNow.AddHours(createInvitationDto.ExpirationHours),
            IsUsed = false
        };

        context.Invitations.Add(invitation);
        await context.SaveChangesAsync(cancellationToken);

        return new InvitationDto
        {
            Id = invitation.Id,
            Token = token,
            CompanyId = invitation.CompanyId,
            CompanyName = company.Name,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed
        };
    }

    public async Task<InvitationDto?> ValidateInvitationAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await context.Invitations
            .Include(i => i.Company)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invitation == null || 
            invitation.IsUsed || 
            invitation.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            Token = invitation.Token,
            CompanyId = invitation.CompanyId,
            CompanyName = invitation.Company.Name,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed,
            UsedAt = invitation.UsedAt,
            UsedByUserId = invitation.UsedByUserId,
            UsedByEmail = invitation.UsedByEmail
        };
    }

    public async Task<bool> MarkInvitationAsUsedAsync(string token, Guid userId, string email, CancellationToken cancellationToken)
    {
        var invitation = await context.Invitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invitation == null || invitation.IsUsed)
        {
            return false;
        }

        invitation.IsUsed = true;
        invitation.UsedAt = DateTime.UtcNow;
        invitation.UsedByUserId = userId;
        invitation.UsedByEmail = email;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
