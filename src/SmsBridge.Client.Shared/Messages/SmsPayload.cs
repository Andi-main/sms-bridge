using System.Text.Json.Serialization;

namespace SmsBridge.Client.Shared.Messages;

public sealed class SmsPayload
{
    [JsonPropertyName("sender")]
    public required string Sender { get; init; }

    [JsonPropertyName("body")]
    public required string Body { get; init; }

    [JsonPropertyName("receivedAt")]
    public DateTimeOffset ReceivedAt { get; init; }
}