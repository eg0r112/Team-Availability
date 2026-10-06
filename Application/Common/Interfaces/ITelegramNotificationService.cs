namespace TeamAvailability.Application.Common.Interfaces;

public interface ITelegramNotificationService
{
    Task SendMessageAsync(string chatId, string message, CancellationToken cancellationToken = default);
    bool IsConfigured { get; }
}
