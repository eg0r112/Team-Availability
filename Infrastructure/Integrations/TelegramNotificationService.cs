using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.Options;

namespace TeamAvailability.Infrastructure.Integrations;

public sealed class TelegramNotificationService(
    HttpClient httpClient,
    IOptions<TelegramOptions> options,
    ILogger<TelegramNotificationService> logger) : ITelegramNotificationService
{
    private readonly TelegramOptions _options = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.BotToken);

    public async Task SendMessageAsync(string chatId, string message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            logger.LogWarning("Telegram bot token is not configured. Skipping notification.");
            return;
        }

        var url = $"https://api.telegram.org/bot{_options.BotToken}/sendMessage";
        var payload = new { chat_id = chatId, text = message };
        var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
