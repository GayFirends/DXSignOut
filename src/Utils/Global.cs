using LiteDB;
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
    public static readonly LiteDatabase Database;

    static Global()
    {
        Config config = Config.GetFromFile(ConfigFilePath) ?? throw new NullReferenceException();
        Config = config;
        I18n = new(LanguagePackPath);
        Database = new(CountDatabasePath);
        BotClient = new(Config.Token,
            string.IsNullOrWhiteSpace(Config.ProxyUrl)
                ? default
                : new(new HttpClientHandler { Proxy = new WebProxy(Config.ProxyUrl, true) }));
    }
}