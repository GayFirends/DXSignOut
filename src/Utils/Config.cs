using LiteDB;
using System.Net;
using Telegram.Bot;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DxSignOut.Utils;

public record Config(string Token, string ProxyUrl, bool EnableAutoL10n, string SignOutApiUrl)
{
    internal const string ExceptionLoggingPath = "./Exception.log";
    internal const string CountDatabasePath = "./History.db";
    internal const string ConfigFilePath = "./Config.json";
    internal const string LanguagePackPath = "./LanguagePack/";
    internal const string IdStart = "SGWCMAID";
    internal static readonly Config Shared;
    internal static readonly L10nProvider L10n;
    internal static readonly TelegramBotClient BotClient;
    internal static readonly LiteDatabase Database;

    static Config()
    {
        Shared = GetFromFile(ConfigFilePath) ?? throw new NullReferenceException();
        L10n = new(LanguagePackPath);
        BotClient = new(Shared.Token,
            string.IsNullOrWhiteSpace(Shared.ProxyUrl)
                ? default
                : new(new HttpClientHandler { Proxy = new WebProxy(Shared.ProxyUrl, true) }));
        Database = new(CountDatabasePath);
    }

    internal static Config? GetFromFile(string path)
    {
        string configStr = FileHelper.CheckFile(path,
            JsonSerializer.Serialize(new Config(string.Empty, string.Empty, default, string.Empty)));
        return JsonSerializer.Deserialize<Config>(configStr);
    }
}