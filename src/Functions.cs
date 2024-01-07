using DxSignOut.MaimaiDX;
using DxSignOut.MaimaiDX.Packet;
using DxSignOut.Utils;
using LiteDB;
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
        List<Task> signOutTasks = [];
        if (message.Text is not null)
        {
            signOutTasks.Add(StartSignOut(message, message.Text));
        }

        if (message.Photo is not null && message.Photo.Length > 0)
        {
            FileInfo file = await Global.BotClient.GetFileAsync(message.Photo[0].FileId);
            if (file.FilePath is not null)
            {
                string path = Path.GetTempFileName();
                string? maiId;
                try
                {
                    await using FileStream stream = File.OpenWrite(path);
                    {
                        await Global.BotClient.DownloadFileAsync(file.FilePath, stream);
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
                    signOutTasks.Add(StartSignOut(message, maiId));
                }
            }
        }

        Task.WaitAll([..signOutTasks]);
    }

    private static async Task StartSignOut(Message message, string maiId)
    {
        if (message.From is null)
        {
            return;
        }

        Internationalization lang = Global.I18n.GetI18n(message.From.LanguageCode);
        Account account = new(maiId);
        Response? data;
        string hash = HashHelper.GetFromString(message.Chat.Id);
        ILiteCollection<HistoryData> dataCollection = Global.Database.GetCollection<HistoryData>(hash);
        Message sentMessage = await Global.BotClient.SendTextMessageAsync(message.Chat.Id, lang["Processing"],
            message.MessageThreadId, ParseMode.MarkdownV2, replyToMessageId: message.MessageId);
        try
        {
            data = await account.SignOutAsync();
        }
        catch (Exception ex)
        {
            await Global.BotClient.EditMessageTextAsync(message.Chat.Id, sentMessage.MessageId,
                lang.Translate("Result", string.Empty, lang["Failed"]), ParseMode.MarkdownV2);
            dataCollection.Insert(new HistoryData(message.Date, SignOutStatus.Failed));
            await File.AppendAllTextAsync(Global.ExceptionLoggingPath,
                $"{(File.Exists(Global.ExceptionLoggingPath) ? '\n' : string.Empty)}[{DateTime.Now}] {ex}");
            return;
        }

        await Global.BotClient.EditMessageTextAsync(message.Chat.Id, sentMessage.MessageId,
            lang.Translate("Result",
                data.UserName is null or "null" ? string.Empty : lang.Translate("Account", data.UserName),
                data.LogoutStatus switch { false => lang["MayFailed"], true => lang["Succeeded"] }),
            ParseMode.MarkdownV2);
        dataCollection.Insert(new HistoryData(message.Date,
            data.LogoutStatus switch { false => SignOutStatus.MayFailed, true => SignOutStatus.Succeeded }));
    }
}