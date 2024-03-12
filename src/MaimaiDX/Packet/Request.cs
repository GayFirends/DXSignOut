using System.Text.Json.Serialization;

namespace DxSignOut.MaimaiDX.Packet;

public record Request([property: JsonPropertyName("data")] string Data);