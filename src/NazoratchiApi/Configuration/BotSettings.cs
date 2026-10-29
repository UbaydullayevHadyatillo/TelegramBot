namespace NazoratchiApi.Configuration;

public sealed class BotSettings
{
    public const string Section = "Bot";

    /// <summary>BotFather bergan token.</summary>
    public string Token { get; set; } = "";

    /// <summary>Serveringizning ochiq HTTPS manzili (oxirida / siz). Masalan: https://bot.mydomain.uz</summary>
    public string WebhookUrl { get; set; } = "";

    /// <summary>Telegram har so'rovda yuboradigan maxfiy kalit (A-Z a-z 0-9 _ -).</summary>
    public string SecretToken { get; set; } = "";

    /// <summary>Har kuni bildirishnoma yuboriladigan vaqt (HH:mm).</summary>
    public string NotifyTime { get; set; } = "08:00";

    public string TimeZone { get; set; } = "Asia/Tashkent";
}
