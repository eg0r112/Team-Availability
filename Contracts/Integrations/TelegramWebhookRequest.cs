namespace TeamAvailability.Contracts.Integrations;

public sealed record TelegramWebhookRequest(TelegramUpdate Update);

public sealed record TelegramUpdate(long UpdateId, TelegramMessage? Message);

public sealed record TelegramMessage(long MessageId, TelegramUser From, TelegramChat Chat, string? Text);

public sealed record TelegramUser(long Id, string? Username, string? FirstName);

public sealed record TelegramChat(long Id, string Type);
