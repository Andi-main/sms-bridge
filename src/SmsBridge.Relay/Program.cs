using Microsoft.AspNetCore.SignalR;
using SmsBridge.Relay.Hubs;
using SmsBridge.Shared.Messages;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "SMS Bridge Relay",
        status = "running"
    });
});

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTimeOffset.UtcNow
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapPost(
        "/dev/messages",
        async (
            RelayMessageEnvelope envelope,
            IHubContext<RelayHub> hubContext,
            CancellationToken cancellationToken) =>
        {
            string channelId =
                envelope.ChannelId?.Trim() ?? string.Empty;

            string payload =
                envelope.Payload?.Trim() ?? string.Empty;

            if (channelId.Length == 0)
            {
                return Results.BadRequest(
                    new { error = "Channel ID is required." });
            }

            if (payload.Length == 0)
            {
                return Results.BadRequest(
                    new { error = "Payload is required." });
            }

            RelayMessageEnvelope outgoingEnvelope = new()
            {
                ChannelId = channelId,
                Payload = payload,
                SentAt = envelope.SentAt == default
                    ? DateTimeOffset.UtcNow
                    : envelope.SentAt
            };

            await hubContext.Clients
                .Group(channelId)
                .SendAsync(
                    "MessageReceived",
                    outgoingEnvelope,
                    cancellationToken);

            return Results.Accepted();
        });
}

app.MapHub<RelayHub>("/hubs/relay");

app.Run();