using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    public Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var parsedUserId))
        {
            return Task.FromResult(Result<UserDto>.Failure("Invalid user ID"));
        }

        var parts = request.UserName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var firstName = request.GivenName ?? (parts.Length > 0 ? parts[0] : "");
        var lastName = request.Surname ?? (parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "");

        var user = new UserDto
        {
            Id = parsedUserId,
            Email = request.UserEmail ?? "",
            FirstName = firstName,
            LastName = lastName,
            CompanyId = Guid.TryParse(request.CompanyId, out var cid) ? cid : Guid.Empty,
            RoleId = Guid.TryParse(request.RoleId, out var rid) ? rid : Guid.Empty,
            RoleName = request.RoleName ?? "",
            IsActive = bool.TryParse(request.IsActive, out var active) && active
        };

        return Task.FromResult(Result<UserDto>.Success(user));
    }
}