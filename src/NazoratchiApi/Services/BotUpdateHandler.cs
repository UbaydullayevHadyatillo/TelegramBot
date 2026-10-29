using NazoratchiApi.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NazoratchiApi.Services;

/// <summary>Webhook orqali kelgan har bir Update shu yerda qayta ishlanadi.</summary>
public sealed class BotUpdateHandler(
    ITelegramBotClient bot,
    RosterService roster,
    ILogger<BotUpdateHandler> logger)
{
    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message?.Text is { } text)
                await OnMessageAsync(update.Message, text, ct);
            else if (update.CallbackQuery is { } callback)
                await OnCallbackAsync(callback, ct);
        }
        catch (Exception ex)
        {
            // Xato bo'lsa ham Telegramga 200 qaytaramiz, aks holda u bir xil update'ni qayta-qayta yuboradi.
            logger.LogError(ex, "Update {UpdateId} ishlovida xato", update.Id);
        }
    }

    private async Task OnMessageAsync(Message message, string text, CancellationToken ct)
    {
        var command = text.Split(' ', 2)[0].Split('@')[0].ToLowerInvariant();

        switch (command)
        {
            case "/start":
            case "/menu":
                await SendMenuAsync(message.Chat.Id, ct);
                break;

            case "/ovqat":
            case "/non":
            case "/bozorlik":
            case "/tozalov":
                await bot.SendMessage(
                    message.Chat.Id,
                    ContentFor(command.TrimStart('/')),
                    parseMode: ParseMode.Html,
                    replyMarkup: MessageBuilder.MenuKeyboard(),
                    cancellationToken: ct);
                break;

            case "/id":
                await bot.SendMessage(
                    message.Chat.Id,
                    $"🆔 Sizning Telegram ID: <code>{message.From?.Id}</code>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
                break;

            default:
                await SendMenuAsync(message.Chat.Id, ct);
                break;
        }
    }

    private async Task OnCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);

        if (callback.Message is null || callback.Data is null)
            return;

        var chatId = callback.Message.Chat.Id;
        var messageId = callback.Message.MessageId;

        if (callback.Data == "menu")
        {
            await bot.EditMessageText(
                chatId, messageId, MessageBuilder.MenuText,
                parseMode: ParseMode.Html,
                replyMarkup: MessageBuilder.MenuKeyboard(),
                cancellationToken: ct);
            return;
        }

        if (callback.Data.StartsWith("duty:"))
        {
            var key = callback.Data["duty:".Length..];
            await bot.EditMessageText(
                chatId, messageId, ContentFor(key),
                parseMode: ParseMode.Html,
                replyMarkup: MessageBuilder.BackKeyboard(),
                cancellationToken: ct);
        }
    }

    private Task SendMenuAsync(long chatId, CancellationToken ct) =>
        bot.SendMessage(
            chatId, MessageBuilder.MenuText,
            parseMode: ParseMode.Html,
            replyMarkup: MessageBuilder.MenuKeyboard(),
            cancellationToken: ct);

    private string ContentFor(string key) => key switch
    {
        "ovqat" => MessageBuilder.Daily(DutyType.Ovqat, roster.GetDaily(DutyType.Ovqat)),
        "non" => MessageBuilder.Daily(DutyType.Non, roster.GetDaily(DutyType.Non)),
        "bozorlik" => MessageBuilder.Shopping(roster.GetShopping()),
        "tozalov" => MessageBuilder.CleaningText,
        _ => MessageBuilder.MenuText
    };
}
