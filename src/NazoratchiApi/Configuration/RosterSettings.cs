namespace NazoratchiApi.Configuration;

public sealed class RosterSettings
{
    public const string Section = "Roster";

    /// <summary>Navbat aylanishi boshlanadigan sana. Birinchi kuni User1 navbatchi bo'ladi.</summary>
    public DateOnly StartDate { get; set; } = new(2026, 11, 1);

    public List<MemberSettings> Members { get; set; } = new();
}

public sealed class MemberSettings
{
    public string Name { get; set; } = "";

    /// <summary>Telegram ID (raqam). Botga /id yozsangiz o'zi aytadi.</summary>
    public long TelegramId { get; set; }
}
