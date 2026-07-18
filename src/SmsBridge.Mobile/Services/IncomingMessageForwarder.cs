using Microsoft.Extensions.Logging;

namespace SmsBridge.Mobile.Services;

public sealed class IncomingMessageForwarder
    : IIncomingMessageForwarder
{
    private readonly RelayConnectionService _relayConnection;
    private readonly ILogger<IncomingMessageForwarder> _logger;

    public IncomingMessageForwarder(
        RelayConnectionService relayConnection,
        ILogger<IncomingMessageForwarder> logger)
    {
        _relayConnection = relayConnection;
        _logger = logger;
    }

    public async Task ForwardAsync(
        string sender,
        string body,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken = default)
    {
        if (!_relayConnection.IsConnected)
        {
            await _relayConnection.ConnectAsync(
                cancellationToken);
        }

        await _relayConnection.SendMessageAsync(
            sender,
            body,
            receivedAt,
            cancellationToken);

        _logger.LogInformation(
            "Forwarded incoming SMS from {Sender}.",
            sender);
    }
}