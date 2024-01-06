using System.Text.Json.Serialization;

namespace DxSignOut.MaimaiDX.Packet;

public record Request
{
    [JsonPropertyName("data")] public required string Data { get; set; }
}