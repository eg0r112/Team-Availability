using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamAvailability.Application.Features.Integrations.Telegram;
using TeamAvailability.Infrastructure.Options;

namespace TeamAvailability.Infrastructure.BackgroundServices;

/// <summary>
/// Локальная разработка: long polling вместо webhook (не нужен публичный URL).
/// </summary>
public sealed class TelegramPollingService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<TelegramOptions> options,
    ILogger<TelegramPollingService> logger) : BackgroundService
{
    private readonly TelegramOptions _options = options.Value;
    private long _offset;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            logger.LogInformation("Telegram bot token not set — polling disabled.");
            return;
        }

        if (_options.UseWebhook)
        {
            logger.LogInformation("Telegram webhook mode enabled — polling disabled.");
            return;
        }

        logger.LogInformation("Telegram long polling started (local mode, no public URL needed).");

        var client = httpClientFactory.CreateClient(nameof(TelegramPollingService));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_options.BotToken}/getUpdates?offset={_offset}&timeout=30";
                using var response = await client.GetAsync(url, stoppingToken);
                response.EnsureSuccessStatusCode();

                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(stoppingToken), cancellationToken: stoppingToken);
                if (!doc.RootElement.GetProperty("ok").GetBoolean())
                    continue;

                foreach (var update in doc.RootElement.GetProperty("result").EnumerateArray())
                {
                    _offset = update.GetProperty("update_id").GetInt64() + 1;

                    if (!update.TryGetProperty("message", out var message))
                        continue;

                    var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();
                    var userId = message.GetProperty("from").GetProperty("id").GetInt64();
                    var text = message.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;

                    using var scope = scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var reply = await mediator.Send(new ProcessTelegramUpdateCommand(chatId, userId, text), stoppingToken);

                    await SendReplyAsync(client, chatId, reply, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Telegram polling error");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task SendReplyAsync(HttpClient client, long chatId, string text, CancellationToken cancellationToken)
    {
        var url = $"https://api.telegram.org/bot{_options.BotToken}/sendMessage";
        var content = new StringContent(
            JsonSerializer.Serialize(new { chat_id = chatId, text }),
            System.Text.Encoding.UTF8,
            "application/json");
        await client.PostAsync(url, content, cancellationToken);
    }
}
