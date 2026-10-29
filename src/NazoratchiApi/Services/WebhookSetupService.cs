using Microsoft.Extensions.Options;
using NazoratchiApi.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NazoratchiApi.Services;

/// <summary>Ilova ishga tushganda webhook'ni Telegramda avtomatik ro'yxatdan o'tkazadi.</summary>
public sealed class WebhookSetupService(
    ITelegramBotClient bot,
    IOptions<BotSettings> options,
    ILogger<WebhookSetupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.WebhookUrl) || settings.WebhookUrl.Contains("SIZNING-DOMEN"))
        {
            logger.LogWarning("Bot:WebhookUrl sozlanmagan — webhook o'rnatilmadi.");
            return;
        }

        var url = settings.WebhookUrl.TrimEnd('/') + "/webhook";

        try
        {
            await bot.SetWebhook(
                url: url,
                secretToken: settings.SecretToken,
                allowedUpdates: new[] { UpdateType.Message, UpdateType.CallbackQuery },
                cancellationToken: ct);

            logger.LogInformation("Webhook o'rnatildi: {Url}", url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Webhook o'rnatib bo'lmadi: {Url}", url);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
