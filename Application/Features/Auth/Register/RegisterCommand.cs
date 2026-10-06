using MediatR;
using TeamAvailability.Application.Common.Behaviors;
using TeamAvailability.Contracts.Auth;

namespace TeamAvailability.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<TokenResponse>, ITransactionalRequest;
