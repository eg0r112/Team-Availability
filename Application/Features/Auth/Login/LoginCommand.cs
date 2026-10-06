using MediatR;
using TeamAvailability.Contracts.Auth;

namespace TeamAvailability.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<TokenResponse>;
