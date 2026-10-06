using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Auth;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken)
            ?? throw new DomainException("Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = RefreshToken.Create(user.Id, jwtTokenService.GenerateRefreshToken(), dateTimeProvider.UtcNow.AddDays(7));
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        return new TokenResponse(accessToken, refreshToken.Token, expiresAt);
    }
}
