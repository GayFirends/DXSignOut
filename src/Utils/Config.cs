using System.Text.Json;

namespace DxSignOut.Utils;

public record Config(string Token, string ProxyUrl, bool EnableAutoI18n, string SignOutApiUrl)
{
    internal static Config? GetFromFile(string path)
    {
        string configStr = FileHelper.CheckFile(path,
            JsonSerializer.Serialize(new Config(string.Empty, string.Empty, default, string.Empty)));
        return JsonSerializer.Deserialize<Config>(configStr);
    }
}