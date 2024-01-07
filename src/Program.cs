using DxSignOut;
using DxSignOut.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

Config.BotClient.StartReceiving(async (_, update, cancellationToken) =>
{
    try
    {
        switch (update.Type)
        {
            case UpdateType.Unknown:
            case UpdateType.InlineQuery:
            case UpdateType.ChosenInlineResult:
            case UpdateType.CallbackQuery:
            case UpdateType.EditedMessage:
            case UpdateType.ChannelPost:
            case UpdateType.EditedChannelPost:
            case UpdateType.ShippingQuery:
            case UpdateType.PreCheckoutQuery:
            case UpdateType.Poll:
            case UpdateType.PollAnswer:
            case UpdateType.MyChatMember:
            case UpdateType.ChatMember:
            case UpdateType.ChatJoinRequest:
            case UpdateType.Message when update.Message?.Type is not (MessageType.Text or MessageType.Photo) ||
                                         update.Message.Chat.Id != update.Message.From?.Id:
                {
                    break;
                }
            case UpdateType.Message:
                {
                    await update.Message.OnReceived();
                    break;
                }
            default:
                {
                    throw new NotSupportedException();
                }
        }
    }
    catch (Exception ex)
    {
        await File.AppendAllTextAsync(Config.ExceptionLoggingPath,
            $"{(File.Exists(Config.ExceptionLoggingPath) ? '\n' : string.Empty)}[{DateTime.Now}] {ex}",
            cancellationToken);
    }
}, (_, _, _) => { });
Thread.CurrentThread.Join();