namespace SmsBridge.Mobile.Models;

public sealed class SmsPayload
{
    public required string Sender { get; init; }

    public required string Body { get; init; }

    public DateTimeOffset ReceivedAt { get; init; }
}