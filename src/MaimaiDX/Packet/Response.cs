using System.Text.Json.Serialization;

namespace DxSignOut.MaimaiDX.Packet;

public record Response
{
    [JsonPropertyName("LogoutStatus")] public required bool LogoutStatus { get; set; }

    [JsonPropertyName("UserName")] public required string? UserName { get; set; }
}