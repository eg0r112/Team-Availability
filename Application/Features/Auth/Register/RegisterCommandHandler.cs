using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Auth;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<RegisterCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
            throw new ConflictException("User with this email already exists.");

        var user = User.Create(email, request.DisplayName, passwordHasher.Hash(request.Password));
        db.Users.Add(user);

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = RefreshToken.Create(user.Id, jwtTokenService.GenerateRefreshToken(), dateTimeProvider.UtcNow.AddDays(7));
        db.RefreshTokens.Add(refreshToken);

        return new TokenResponse(accessToken, refreshToken.Token, expiresAt);
    }
}
