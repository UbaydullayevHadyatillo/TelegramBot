using NazoratchiApi.Configuration;

namespace NazoratchiApi.Models;

/// <summary>Kunlik navbat (ovqat, non): kecha / bugun / ertaga.</summary>
public sealed record DailyView(MemberSettings Yesterday, MemberSettings Today, MemberSettings Tomorrow);

public sealed record ShopPair(MemberSettings First, MemberSettings Second);

/// <summary>Bozorlik: oxirgi / eng yaqin / keyingi yakshanba.</summary>
public sealed record ShoppingView(
    DateOnly LastSunday, ShopPair Last,
    DateOnly NextSunday, ShopPair Next,
    DateOnly AfterSunday, ShopPair After,
    bool NextIsToday);
