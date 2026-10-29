using System.Net;
using NazoratchiApi.Configuration;
using NazoratchiApi.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace NazoratchiApi.Services;

/// <summary>Botdagi barcha matnlar va tugmalar shu yerda (o'zgartirish oson bo'lsin deb).</summary>
public static class MessageBuilder
{
    public const string MenuText =
        "👋 <b>Nazoratchi</b> botiga xush kelibsiz!\n\nQuyidagi bo'limlardan birini tanlang:";

    public const string CleaningText =
        "🧹 <b>Tozalov navbati</b>\n\n🛠 Hali ishlanmoqda. Tez orada qo'shiladi!";

    private static string E(string s) => WebUtility.HtmlEncode(s);

    private static string D(DateOnly d) => d.ToString("dd.MM.yyyy");

    public static InlineKeyboardMarkup MenuKeyboard() => new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("🍲 Ovqat navbati", "duty:ovqat"),
            InlineKeyboardButton.WithCallbackData("🍞 Non navbati", "duty:non")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("🛒 Bozorlik navbati", "duty:bozorlik"),
            InlineKeyboardButton.WithCallbackData("🧹 Tozalov navbati", "duty:tozalov")
        }
    });

    public static InlineKeyboardMarkup BackKeyboard() => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("⬅️ Orqaga", "menu") }
    });

    public static string Daily(DutyType type, DailyView v)
    {
        var (emoji, title) = type == DutyType.Ovqat
            ? ("🍲", "Ovqat navbati")
            : ("🍞", "Non navbati");

        return $"{emoji} <b>{title}</b>\n\n" +
               $"📅 Bugun: <b>{E(v.Today.Name)}</b>\n" +
               $"➡️ Ertaga: <b>{E(v.Tomorrow.Name)}</b>\n" +
               $"✅ Oxirgi bo'lib qilgan (kecha): <b>{E(v.Yesterday.Name)}</b>";
    }

    public static string Shopping(ShoppingView v)
    {
        var nextLabel = v.NextIsToday ? "Bugun" : "Eng yaqin yakshanba";
        return "🛒 <b>Bozorlik navbati</b>\n<i>Har yakshanba 2 kishi bozorlik qiladi</i>\n\n" +
               $"📅 {nextLabel} ({D(v.NextSunday)}): <b>{Pair(v.Next)}</b>\n" +
               $"➡️ Keyingi yakshanba ({D(v.AfterSunday)}): <b>{Pair(v.After)}</b>\n" +
               $"✅ Oxirgi bo'lib qilgan ({D(v.LastSunday)}): <b>{Pair(v.Last)}</b>";
    }

    public static string Pair(ShopPair p) => $"{E(p.First.Name)} va {E(p.Second.Name)}";

    /// <summary>Navbatchiga shaxsiy chatga yuboriladigan kunlik eslatma.</summary>
    public static string Reminder(
        MemberSettings member, bool cooking, bool bread, ShopPair? shopping)
    {
        var lines = new List<string>();
        if (cooking) lines.Add("🍲 Ovqat tayyorlash");
        if (bread) lines.Add("🍞 Non olib kelish");
        if (shopping is not null)
        {
            var partner = ReferenceEquals(shopping.First, member) ? shopping.Second : shopping.First;
            lines.Add($"🛒 Bozorlik (yakshanba) — sherigingiz: {E(partner.Name)}");
        }

        return $"🔔 <b>Salom, {E(member.Name)}!</b>\n\nBugun sening navbating:\n" +
               string.Join("\n", lines.Select(l => "• " + l));
    }
}
