using Microsoft.AspNetCore.SignalR.Client;
using SmsBridge.Desktop.Models;
using SmsBridge.Shared.Messages;
using System.Text.Json;
using SmsBridge.Desktop.Contracts;
namespace SmsBridge.Desktop.Services;

public sealed class RelayConnectionService : BackgroundService, IAsyncDisposable
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly HubConnection _connection;
    private readonly IMessageStore _messageStore;
    private readonly ILogger<RelayConnectionService> _logger;
    private readonly string _channelId;
    private readonly IDisposable _messageSubscription;

    public RelayConnectionService(
        IMessageStore messageStore,
        IConfiguration configuration,
        ILogger<RelayConnectionService> logger)
    {
        _messageStore = messageStore;
        _logger = logger;

        string relayBaseUrl =
            configuration["Relay:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException(
                "Relay:BaseUrl is not configured.");

        _channelId =
            configuration["Relay:ChannelId"]?.Trim()
            ?? throw new InvalidOperationException(
                "Relay:ChannelId is not configured.");

        if (_channelId.Length == 0)
        {
            throw new InvalidOperationException(
                "Relay:ChannelId cannot be empty.");
        }

        _connection = new HubConnectionBuilder()
            .WithUrl($"{relayBaseUrl}/hubs/relay")
            .WithAutomaticReconnect()
            .Build();

        _messageSubscription =
            _connection.On<RelayMessageEnvelope>(
                "MessageReceived",
                HandleMessageReceived);

        _connection.Reconnected += async _ =>
        {
            await JoinChannelAsync(CancellationToken.None);

            _logger.LogInformation(
                "Reconnected to Relay channel {ChannelId}.",
                _channelId);
        };

        _connection.Closed += exception =>
        {
            _logger.LogWarning(
                exception,
                "Relay connection was closed.");

            return Task.CompletedTask;
        };
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _connection.StartAsync(stoppingToken);
                    await JoinChannelAsync(stoppingToken);

                    _logger.LogInformation(
                        "Connected to Relay channel {ChannelId}.",
                        _channelId);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Could not connect to Relay. Retrying...");
                }
            }

            await Task.Delay(RetryDelay, stoppingToken);
        }
    }

    private Task JoinChannelAsync(
        CancellationToken cancellationToken)
    {
        return _connection.InvokeAsync(
            "JoinChannel",
            _channelId,
            cancellationToken);
    }

    private void HandleMessageReceived(
    RelayMessageEnvelope envelope)
    {
        try
        {
            SmsPayload? payload =
                JsonSerializer.Deserialize<SmsPayload>(
                    envelope.Payload);

            string sender =
                payload?.Sender?.Trim()
                ?? string.Empty;

            string body =
                payload?.Body?.Trim()
                ?? string.Empty;

            if (sender.Length == 0 || body.Length == 0)
            {
                _logger.LogWarning(
                    "Received an invalid SMS payload from Relay.");

                return;
            }

            _messageStore.AddMessage(new SmsMessage
            {
                Sender = sender,
                Body = body,
                ReceivedAt =
                    payload?.ReceivedAt?.LocalDateTime
                    ?? envelope.SentAt.LocalDateTime
            });
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Received malformed JSON payload from Relay.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _messageSubscription.Dispose();
        await _connection.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}