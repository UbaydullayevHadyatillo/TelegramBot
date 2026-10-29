using Microsoft.Extensions.Options;
using NazoratchiApi.Configuration;

namespace NazoratchiApi.Services;

/// <summary>Toshkent (yoki sozlangan) vaqt zonasi bo'yicha hozirgi vaqt.</summary>
public sealed class Clock
{
    private readonly TimeZoneInfo _zone;

    public Clock(IOptions<BotSettings> options)
    {
        try
        {
            _zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
        }
        catch
        {
            _zone = TimeZoneInfo.CreateCustomTimeZone("UZT", TimeSpan.FromHours(5), "UZT", "UZT");
        }
    }

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zone);

    public DateOnly Today => DateOnly.FromDateTime(Now);
}
