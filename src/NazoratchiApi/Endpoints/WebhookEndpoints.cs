using Microsoft.Extensions.Options;
using NazoratchiApi.Configuration;
using NazoratchiApi.Services;
using Telegram.Bot.Types;

namespace NazoratchiApi.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapBotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "Nazoratchi bot ishlayapti ✅");

        // Telegram shu manzilga update yuboradi.
        app.MapPost("/webhook", async (
            Update update,
            HttpRequest request,
            BotUpdateHandler handler,
            IOptions<BotSettings> options,
            CancellationToken ct) =>
        {
            var secret = request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
            if (secret != options.Value.SecretToken)
                return Results.Unauthorized();

            await handler.HandleAsync(update, ct);
            return Results.Ok();
        });

        // Test uchun: kunlik xabarni hoziroq yuborish.
        app.MapPost("/admin/notify-now", async (
            HttpRequest request,
            DailyNotifierService notifier,
            IOptions<BotSettings> options,
            CancellationToken ct) =>
        {
            if (request.Headers["X-Admin-Secret"].ToString() != options.Value.SecretToken)
                return Results.Unauthorized();

            var sent = await notifier.NotifyAsync(ct);
            return Results.Ok(new { sent });
        });

        return app;
    }
}
