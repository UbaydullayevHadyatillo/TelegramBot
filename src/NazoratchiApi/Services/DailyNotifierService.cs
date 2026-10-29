using Microsoft.Extensions.Options;
using NazoratchiApi.Configuration;
using NazoratchiApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NazoratchiApi.Services;

/// <summary>Har kuni belgilangan vaqtda navbatchilarga shaxsiy chatga xabar yuboradi.</summary>
public sealed class DailyNotifierService(
    ITelegramBotClient bot,
    RosterService roster,
    Clock clock,
    IOptions<BotSettings> options,
    ILogger<DailyNotifierService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var time = TimeOnly.TryParse(options.Value.NotifyTime, out var t) ? t : new TimeOnly(8, 0);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = clock.Now;
                var next = now.Date + time.ToTimeSpan();
                if (next <= now) next = next.AddDays(1);

                await Task.Delay(next - now, stoppingToken);

                try
                {
                    await NotifyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kunlik bildirishnomada xato");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ilova to'xtatilmoqda
        }
    }

    /// <summary>Bugungi navbatchilarni aniqlab, xabar yuboradi. /admin/notify-now ham shuni chaqiradi.</summary>
    public async Task<int> NotifyAsync(CancellationToken ct)
    {
        var today = clock.Today;
        var cook = roster.DailyPerson(DutyType.Ovqat, today);
        var bread = roster.DailyPerson(DutyType.Non, today);
        var shopping = today.DayOfWeek == DayOfWeek.Sunday ? roster.ShoppingPair(today) : null;

        var sent = 0;

        foreach (var member in roster.Members)
        {
            var isCook = ReferenceEquals(member, cook);
            var isBread = ReferenceEquals(member, bread);
            var shop = shopping is not null &&
                       (ReferenceEquals(member, shopping.First) || ReferenceEquals(member, shopping.Second))
                ? shopping
                : null;

            if (!isCook && !isBread && shop is null)
                continue;

            if (member.TelegramId == 0)
            {
                logger.LogWarning("{Name} uchun TelegramId kiritilmagan — xabar yuborilmadi.", member.Name);
                continue;
            }

            try
            {
                await bot.SendMessage(
                    member.TelegramId,
                    MessageBuilder.Reminder(member, isCook, isBread, shop),
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                sent++;
            }
            catch (Exception ex)
            {
                // Odatda: foydalanuvchi botga /start bosmagan.
                logger.LogWarning(ex, "{Name} ga xabar yuborib bo'lmadi (botga /start bosganmi?)", member.Name);
            }
        }

        return sent;
    }
}
