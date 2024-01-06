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
        Task? signOutTask1 = default;
        Task? signOutTask2 = default;
        if (message.Photo is not null && message.Photo.Length > 0)
        {
            FileInfo file = await Global.BotClient.GetFileAsync(message.Photo[0].FileId);
            if (file.FilePath is not null)
            {
                DirectoryInfo dir =
                    FileHelper.CheckDir(Path.Combine(Global.CachePath, HashHelper.GetFromString(message.Chat.Id)));
                string path = Path.Combine(dir.FullName, Path.GetFileName(file.FilePath));
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
                    dir.Delete(true);
                }

                if (maiId is not null)
                {
                    signOutTask1 = StartSignOut(message, maiId);
                }
            }
        }

        if (message.Text is not null)
        {
            signOutTask2 = StartSignOut(message, message.Text);
        }

        if (signOutTask1 is not null)
        {
            await signOutTask1;
        }

        if (signOutTask2 is not null)
        {
            await signOutTask2;
        }
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
        using LiteDatabase database = new(Global.CountDatabasePath);
        ILiteCollection<HistoryData> dataCollection =
            database.GetCollection<HistoryData>(HashHelper.GetFromString(message.Chat.Id));
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