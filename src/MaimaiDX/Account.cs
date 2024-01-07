using DxSignOut.MaimaiDX.Packet;
using DxSignOut.Utils;
using System.Net.Http.Json;
using System.Text.Json;

namespace DxSignOut.MaimaiDX;

internal class Account
{
    private readonly string _id;

    public Account(string id)
    {
        if (id.Length is not 84 || !id.StartsWith(Config.IdStart))
        {
            throw new ArgumentException(id);
        }

        _id = id;
    }

    public async Task<Response> SignOutAsync()
    {
        using HttpClient client = new();
        string url = string.Format(Config.Shared.SignOutApiUrl, _id);
        Request requestData = new() { Data = _id };
        using HttpResponseMessage response = await client.PostAsJsonAsync(url, requestData);
        try
        {
            return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Response>() ??
                   throw new NullReferenceException();
        }
        catch (JsonException ex)
        {
            string request = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(request, ex);
        }
    }
}