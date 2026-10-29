using NazoratchiApi.Configuration;
using NazoratchiApi.Endpoints;
using NazoratchiApi.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// ---------- Sozlamalar ----------
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection(BotSettings.Section));
builder.Services.Configure<RosterSettings>(builder.Configuration.GetSection(RosterSettings.Section));

var botSettings = builder.Configuration.GetSection(BotSettings.Section).Get<BotSettings>() ?? new BotSettings();
var rosterSettings = builder.Configuration.GetSection(RosterSettings.Section).Get<RosterSettings>() ?? new RosterSettings();

if (string.IsNullOrWhiteSpace(botSettings.Token))
    throw new InvalidOperationException("Bot:Token kiritilmagan. Environment variable: Bot__Token");

if (rosterSettings.Members.Count < 2)
    throw new InvalidOperationException("Roster:Members kamida 2 kishidan iborat bo'lishi kerak.");

// ---------- Telegram.Bot ----------
// Telegram Update'ini Minimal API to'g'ri o'qishi uchun JSON sozlamasi.
builder.Services.ConfigureHttpJsonOptions(o => JsonBotAPI.Configure(o.SerializerOptions));
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botSettings.Token));

// ---------- Servislar ----------
builder.Services.AddSingleton<Clock>();
builder.Services.AddSingleton<RosterService>();
builder.Services.AddSingleton<BotUpdateHandler>();

builder.Services.AddHostedService<WebhookSetupService>();
builder.Services.AddSingleton<DailyNotifierService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DailyNotifierService>());

var app = builder.Build();

app.MapBotEndpoints();

app.Run();
