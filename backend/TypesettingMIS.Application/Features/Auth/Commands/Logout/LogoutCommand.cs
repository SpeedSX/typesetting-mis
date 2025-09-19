using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;