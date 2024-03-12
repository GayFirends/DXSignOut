using DxSignOut.MaimaiDX;
using DxSignOut.MaimaiDX.Packet;
using DxSignOut.Utils;
using LiteDB;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using FileInfo = Telegram.Bot.Types.File;

namespace DxSignOut;

internal static class Functions
{
    public static async Task OnReceived(this Message message)
    {
        if (message.From is null)
        {
            return;
        }

        Localizer lang = Config.L10n.GetLocalizer(message.From.LanguageCode);
        List<Task> signOutTasks = [];
        if (message.Text is not null)
        {
            signOutTasks.Add(StartSignOut(lang, message.Chat.Id, message.MessageId, message.Text));
        }

        if (message.Photo is not null && message.Photo.Length > 0)
        {
            FileInfo file = await Config.BotClient.GetFileAsync(message.Photo[0].FileId);
            if (file.FilePath is not null)
            {
                string path = Path.GetTempFileName();
                string? maiId;
                try
                {
                    await using FileStream stream = File.OpenWrite(path);
                    {
                        await Config.BotClient.DownloadFileAsync(file.FilePath, stream);
                        stream.Close();
                    }
                    maiId = QrCodeHelper.Decode(path);
                }
                finally
                {
                    File.Delete(path);
                }

                if (maiId is not null)
                {
                    signOutTasks.Add(StartSignOut(lang, message.Chat.Id, message.MessageId, maiId));
                }
            }
        }

        Task.WaitAll([.. signOutTasks]);
    }

    private static async Task StartSignOut(Localizer lang, long userId, int messageId, string maiId)
    {
        if (int.TryParse(maiId, out _) && maiId.Length is 8)
        {
            await Config.BotClient.SendTextMessageAsync(userId, lang.Translate("IsUserId", Config.IdStart),
                parseMode: ParseMode.MarkdownV2, replyToMessageId: messageId);
            return;
        }

        Account account = new(maiId);
        Response? data;
        string hash = HashHelper.GetFromString(userId);
        ILiteCollection<HistoryData> dataCollection = Config.Database.GetCollection<HistoryData>(hash);
        Message sentMessage = await Config.BotClient.SendTextMessageAsync(userId, lang["Requesting"],
            parseMode: ParseMode.MarkdownV2, replyToMessageId: messageId);
        Stopwatch stopWatch = Stopwatch.StartNew();
        try
        {
            data = await account.SignOutAsync();
        }
        catch (Exception ex)
        {
            stopWatch.Stop();
            await Config.BotClient.EditMessageTextAsync(userId, sentMessage.MessageId,
                lang.Translate("Result", string.Empty, lang["Failed"],
                    lang.Translate("Minute", stopWatch.Elapsed.TotalMinutes),
                    lang.Translate("Second", stopWatch.Elapsed.Seconds)), ParseMode.MarkdownV2);
            dataCollection.Insert(new HistoryData(account.RequestTime, SignOutStatus.Failed, stopWatch.Elapsed));
            await File.AppendAllTextAsync(Config.ExceptionLoggingPath,
                $"{(File.Exists(Config.ExceptionLoggingPath) ? '\n' : string.Empty)}[{DateTime.Now}] {ex}");
            return;
        }

        stopWatch.Stop();
        await Config.BotClient.EditMessageTextAsync(userId, sentMessage.MessageId,
            lang.Translate("Result",
                data.UserName is null or "null" ? string.Empty : lang.Translate("Account", data.UserName),
                data.LogoutStatus switch { false => lang["MayFailed"], true => lang["Succeeded"] },
                lang.Translate("Minute", stopWatch.Elapsed.TotalMinutes),
                lang.Translate("Second", stopWatch.Elapsed.Seconds)), ParseMode.MarkdownV2);
        dataCollection.Insert(new HistoryData(account.RequestTime,
            data.LogoutStatus switch { false => SignOutStatus.MayFailed, true => SignOutStatus.Succeeded },
            stopWatch.Elapsed));
    }
}