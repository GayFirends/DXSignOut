using System.Security.Cryptography;
using System.Text;

namespace DxSignOut.Utils;

internal static class HashHelper
{
    public static string Get(byte[] bytes)
    {
        byte[] hashBytes = SHA256.HashData(bytes);
        StringBuilder stringBuilder = new();
        foreach (byte hash in hashBytes)
        {
            stringBuilder.Append(Convert.ToChar('a' + hash / 16));
            stringBuilder.Append(Convert.ToChar('a' + hash % 16));
        }

        return stringBuilder.ToString();
    }

    public static string GetFromString(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return Get(bytes);
    }

    public static string GetFromString(object obj)
    {
        string? str = obj.ToString();
        return GetFromString(str ?? throw new NullReferenceException());
    }
}