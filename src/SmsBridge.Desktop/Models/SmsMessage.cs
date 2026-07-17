namespace SmsBridge.Desktop.Models;

public sealed class SmsMessage
{
    public required string Sender { get; init; }

    public required string Body {  get; init; }

    public DateTime ReceivedAt { get; init; }
}
