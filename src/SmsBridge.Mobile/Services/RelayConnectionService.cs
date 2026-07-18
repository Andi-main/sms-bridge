using System.Text.Json;

using Microsoft.AspNetCore.SignalR.Client;

using SmsBridge.Client.Shared.Messages;
using SmsBridge.Mobile.Configuration;
using SmsBridge.Shared.Messages;

namespace SmsBridge.Mobile.Services;

public sealed class RelayConnectionService : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HubConnection _connection;
    private readonly string _channelId;

    public RelayConnectionService(RelayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _channelId = options.ChannelId;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{options.BaseUrl.TrimEnd('/')}/hubs/relay")
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += exception =>
        {
            ConnectionStatusChanged?.Invoke("Reconnecting");

            return Task.CompletedTask;
        };

        _connection.Reconnected += async _ =>
        {
            await JoinChannelAsync(CancellationToken.None);

            ConnectionStatusChanged?.Invoke("Connected");
        };

        _connection.Closed += exception =>
        {
            ConnectionStatusChanged?.Invoke("Disconnected");

            return Task.CompletedTask;
        };
    }

    public event Action<string>? ConnectionStatusChanged;

    public bool IsConnected =>
        _connection.State == HubConnectionState.Connected;

    public async Task ConnectAsync(
        CancellationToken cancellationToken = default)
    {
        if (_connection.State != HubConnectionState.Disconnected)
        {
            return;
        }

        ConnectionStatusChanged?.Invoke("Connecting");

        await _connection.StartAsync(cancellationToken);
        await JoinChannelAsync(cancellationToken);

        ConnectionStatusChanged?.Invoke("Connected");
    }

    public Task SendMessageAsync(
        string sender,
        string body,
        CancellationToken cancellationToken = default)
    {
        return SendMessageAsync(
            sender,
            body,
            DateTimeOffset.Now,
            cancellationToken);
    }

    public async Task SendMessageAsync(
        string sender,
        string body,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException(
                "Connect to Relay before sending a message.");
        }

        string normalizedSender = sender.Trim();
        string normalizedBody = body.Trim();

        if (normalizedSender.Length == 0)
        {
            throw new ArgumentException(
                "Sender cannot be empty.",
                nameof(sender));
        }

        if (normalizedBody.Length == 0)
        {
            throw new ArgumentException(
                "Message body cannot be empty.",
                nameof(body));
        }

        SmsPayload payload = new()
        {
            Sender = normalizedSender,
            Body = normalizedBody,
            ReceivedAt = receivedAt
        };

        RelayMessageEnvelope envelope = new()
        {
            ChannelId = _channelId,
            Payload = JsonSerializer.Serialize(
                payload,
                JsonOptions),
            SentAt = DateTimeOffset.UtcNow
        };

        await _connection.InvokeAsync(
            "SendMessage",
            envelope,
            cancellationToken);
    }

    private Task JoinChannelAsync(
        CancellationToken cancellationToken)
    {
        return _connection.InvokeAsync(
            "JoinChannel",
            _channelId,
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}