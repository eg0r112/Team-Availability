using TeamAvailability.Domain.Common;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Domain.Entities;

public sealed class User : AuditableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public SystemRole Role { get; private set; } = SystemRole.Member;
    public string? TelegramChatId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<TeamMember> TeamMemberships { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public static User Create(string email, string displayName, string passwordHash, SystemRole role = SystemRole.Member)
    {
        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void LinkTelegram(string chatId) => TelegramChatId = chatId;

    public void Deactivate() => IsActive = false;
}
