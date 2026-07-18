using System.Text.Json.Serialization;

namespace SmsBridge.Client.Shared.Messages;

public sealed class EncryptedPayload
{
    [JsonPropertyName("version")]
    public int Version { get; init; }

    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    [JsonPropertyName("nonce")]
    public required string Nonce { get; init; }

    [JsonPropertyName("ciphertext")]
    public required string Ciphertext { get; init; }

    [JsonPropertyName("tag")]
    public required string Tag { get; init; }
}