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
            stringBuilder.Append($"{hash:x2}");
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
        if (str is null)
        {
            throw new NullReferenceException();
        }

        return GetFromString(str);
    }
}