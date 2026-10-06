namespace TeamAvailability.Infrastructure.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; init; } = string.Empty;
    public string? WebhookSecret { get; init; }

    /// <summary>false = long polling (локально, без публичного URL). true = webhook (прод).</summary>
    public bool UseWebhook { get; init; }
}
