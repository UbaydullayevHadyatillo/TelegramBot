using Microsoft.Extensions.Options;
using NazoratchiApi.Configuration;
using NazoratchiApi.Models;

namespace NazoratchiApi.Services;

/// <summary>
/// Navbatni hisoblaydi. Baza kerak emas: navbat sanadan hisoblanadi,
/// shuning uchun server qayta ishga tushsa ham hech narsa yo'qolmaydi.
/// </summary>
public sealed class RosterService
{
    private readonly RosterSettings _settings;
    private readonly Clock _clock;

    public RosterService(IOptions<RosterSettings> options, Clock clock)
    {
        _settings = options.Value;
        _clock = clock;
    }

    public IReadOnlyList<MemberSettings> Members => _settings.Members;

    private int Count => _settings.Members.Count;

    private static int Mod(int a, int n) => ((a % n) + n) % n;

    // ---------- Ovqat va Non (har kuni bitta kishi) ----------

    public MemberSettings DailyPerson(DutyType type, DateOnly date)
    {
        // Non navbati ovqatdan yarim aylanaga siljitilgan: bir kishiga ikkalasi bir kunda tushmaydi.
        var offset = type == DutyType.Non ? Count / 2 : 0;
        var days = date.DayNumber - _settings.StartDate.DayNumber;
        return _settings.Members[Mod(days + offset, Count)];
    }

    public DailyView GetDaily(DutyType type)
    {
        var today = _clock.Today;
        return new DailyView(
            DailyPerson(type, today.AddDays(-1)),
            DailyPerson(type, today),
            DailyPerson(type, today.AddDays(1)));
    }

    // ---------- Bozorlik (har yakshanba 2 kishi) ----------

    public static DateOnly NextSundayOnOrAfter(DateOnly date) =>
        date.AddDays(((int)DayOfWeek.Sunday - (int)date.DayOfWeek + 7) % 7);

    public ShopPair ShoppingPair(DateOnly sunday)
    {
        var firstSunday = NextSundayOnOrAfter(_settings.StartDate);
        var week = (sunday.DayNumber - firstSunday.DayNumber) / 7;
        return new ShopPair(
            _settings.Members[Mod(2 * week, Count)],
            _settings.Members[Mod(2 * week + 1, Count)]);
    }

    public ShoppingView GetShopping()
    {
        var today = _clock.Today;
        var next = NextSundayOnOrAfter(today);
        var last = next.AddDays(-7);
        var after = next.AddDays(7);
        return new ShoppingView(
            last, ShoppingPair(last),
            next, ShoppingPair(next),
            after, ShoppingPair(after),
            next == today);
    }
}
