using System.Net;
using Telegram.Bot;

namespace DxSignOut.Utils;

internal static class Global
{
    public const string ExceptionLoggingPath = "./Exception.log";
    public const string CountDatabasePath = "./History.db";
    public const string ConfigFilePath = "./Config.json";
    public const string LanguagePackPath = "./LanguagePack/";
    public const string IdStart = "SGWCMAID";
    public static readonly Config Config;
    public static readonly I18nHelper I18n;
    public static readonly TelegramBotClient BotClient;

    static Global()
    {
        Config config = Config.GetFromFile(ConfigFilePath) ?? throw new NullReferenceException();
        Config = config;
        I18n = new(LanguagePackPath);
        BotClient = new(Config.Token,
            string.IsNullOrWhiteSpace(Config.ProxyUrl)
                ? default
                : new(new HttpClientHandler { Proxy = new WebProxy(Config.ProxyUrl, true) }));
    }
}