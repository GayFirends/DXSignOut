using System.Text.Json.Serialization;

namespace DxSignOut.MaimaiDX.Packet;

public record Response(
    [property: JsonPropertyName("LogoutStatus")]
    bool LogoutStatus,
    [property: JsonPropertyName("UserName")]
    string? UserName);