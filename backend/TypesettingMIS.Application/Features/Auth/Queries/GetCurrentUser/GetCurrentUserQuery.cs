using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(
    string UserId,
    string? UserEmail,
    string? UserName,
    string? GivenName,
    string? Surname,
    string? CompanyId,
    string? RoleId,
    string? RoleName,
    string? IsActive) : IRequest<Result<UserDto>>;