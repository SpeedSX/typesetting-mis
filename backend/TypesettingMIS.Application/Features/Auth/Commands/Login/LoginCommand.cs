using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Auth;

namespace TypesettingMIS.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;