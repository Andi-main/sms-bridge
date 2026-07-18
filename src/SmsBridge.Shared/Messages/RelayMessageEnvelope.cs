namespace SmsBridge.Shared.Messages;

public sealed class RelayMessageEnvelope
{
    public required string ChannelId { get; init; }

    public required string Payload { get; init; }

    public DateTimeOffset SentAt { get; init; }
}