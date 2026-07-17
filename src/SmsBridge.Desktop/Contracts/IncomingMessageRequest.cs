namespace SmsBridge.Desktop.Contracts;

public class IncomingMessageRequest
{
    public string? Sender { get; init; }
    public string? Body { get; init; }
    public DateTime? ReceivedAt {  get; init; }
}
