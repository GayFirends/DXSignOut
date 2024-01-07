using LiteDB;
using System.Net;
using Telegram.Bot;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DxSignOut.Utils;

public record Config(string Token, string ProxyUrl, bool EnableAutoI18n, string SignOutApiUrl)
{
    public const string ExceptionLoggingPath = "./Exception.log";
    public const string CountDatabasePath = "./History.db";
    public const string ConfigFilePath = "./Config.json";
    public const string LanguagePackPath = "./LanguagePack/";
    public const string IdStart = "SGWCMAID";
    public static readonly Config Shared;
    public static readonly I18nHelper I18n;
    public static readonly TelegramBotClient BotClient;
    public static readonly LiteDatabase Database;

    static Config()
    {
        Config config = GetFromFile(ConfigFilePath) ?? throw new NullReferenceException();
        Shared = config;
        I18n = new(LanguagePackPath);
        Database = new(CountDatabasePath);
        BotClient = new(config.Token,
            string.IsNullOrWhiteSpace(config.ProxyUrl)
                ? default
                : new(new HttpClientHandler { Proxy = new WebProxy(config.ProxyUrl, true) }));
    }

    internal static Config? GetFromFile(string path)
    {
        string configStr = FileHelper.CheckFile(path,
            JsonSerializer.Serialize(new Config(string.Empty, string.Empty, default, string.Empty)));
        return JsonSerializer.Deserialize<Config>(configStr);
    }
}