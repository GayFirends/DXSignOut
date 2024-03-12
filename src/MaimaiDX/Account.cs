using DxSignOut.MaimaiDX.Packet;
using DxSignOut.Utils;
using System.Net.Http.Json;
using System.Text.Json;

namespace DxSignOut.MaimaiDX;

internal class Account
{
    private readonly Request _request;
    private readonly string _url;

    public DateTime RequestTime;

    public Account(string id)
    {
        if (id.Length is not 84 || !id.StartsWith(Config.IdStart))
        {
            throw new ArgumentException(id);
        }

        _url = string.Format(Config.Shared.SignOutApiUrl, id);
        _request = new(id);
    }

    public async Task<Response> SignOutAsync()
    {
        using HttpClient client = new();
        using HttpResponseMessage response = await client.PostAsJsonAsync(_url, _request);
        RequestTime = DateTime.Now;
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