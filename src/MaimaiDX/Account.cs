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
        if (id.Length is not 84 || !id.StartsWith(Global.IdStart))
        {
            throw new ArgumentException(id);
        }

        _id = id;
    }

    public async Task<Response> SignOutAsync()
    {
        using HttpClient client = new();
        using HttpResponseMessage response =
            await client.PostAsJsonAsync<Request>(string.Format(Global.Config.SignOutApiUrl, _id),
                new() { Data = _id });
        try
        {
            Response? data = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Response>();
            return data ?? throw new NullReferenceException();
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync(), ex);
        }
    }
}