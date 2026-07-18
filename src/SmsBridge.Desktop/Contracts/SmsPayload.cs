using System.Text.Json.Serialization;

namespace SmsBridge.Desktop.Contracts;

public sealed class SmsPayload
{
    [JsonPropertyName("sender")]
    public string? Sender { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("receivedAt")]
    public DateTimeOffset? ReceivedAt { get; init; }
}