using Microsoft.AspNetCore.SignalR;

using SmsBridge.Shared.Messages;

namespace SmsBridge.Relay.Hubs;

public sealed class RelayHub : Hub
{
    private const string ChannelContextKey = "channel-id";
    private const int MaxChannelIdLength = 128;
    private const int MaxPayLoadLength = 65_536;

    public async Task JoinChannel(string? channelId)
    {
        string normalizedChannelId = NormalizeChannelId(channelId);

        if (Context.Items.TryGetValue(ChannelContextKey, out object? currentValue) && currentValue is string currentChannelId && !string.Equals(currentChannelId, normalizedChannelId, StringComparison.Ordinal))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentChannelId);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedChannelId);

        Context.Items[ChannelContextKey] = normalizedChannelId;
    }

    public async Task LeaveChannel()
    {
        if (!Context.Items.TryGetValue(ChannelContextKey, out object? currentValue) || currentValue is not string currentChannelId) 
        { 
            return; 
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentChannelId);

        Context.Items.Remove(ChannelContextKey);
    }

    public async Task SendMessage(RelayMessageEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        string channelId = NormalizeChannelId(envelope.ChannelId);

        if (!Context.Items.TryGetValue(ChannelContextKey, out object? currentValue) || currentValue is not string currentChannelId || !string.Equals(currentChannelId, channelId, StringComparison.Ordinal))
        {
            throw new HubException(
                "Join the channel before sending messages.");
        }
        if (string.IsNullOrEmpty(envelope.Payload)) 
        {
            throw new HubException(
                "Payload is Required");
        }
        if (envelope.Payload.Length > MaxPayLoadLength)
        {
            throw new HubException(
                "Payload is too large");
        }

        RelayMessageEnvelope outgoingEnvelope = new()
        {
            ChannelId = channelId,
            Payload = envelope.Payload,
            SentAt = envelope.SentAt == default ? DateTimeOffset.UtcNow : envelope.SentAt
        };

        await Clients.OthersInGroup(channelId).SendAsync("MessageReceived", outgoingEnvelope);
    }

    private static string NormalizeChannelId(string? channelId)
    {
        string normalizeChannelId = channelId?.Trim() ?? string.Empty;

        if (normalizeChannelId.Length == 0)
        {
            throw new HubException("Channel ID is required.");
        }
        if (normalizeChannelId.Length > MaxChannelIdLength)
        {
            throw new HubException("Channel ID is too long.");
        }

        return normalizeChannelId;
    }
}