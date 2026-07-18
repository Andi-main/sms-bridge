using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using SmsBridge.Client.Shared.Messages;
using SmsBridge.Shared.Messages;

string relayBaseUrl =
    args.ElementAtOrDefault(0)?.TrimEnd('/')
    ?? "http://localhost:5111";

string channelId =
    args.ElementAtOrDefault(1)?.Trim()
    ?? "demo-channel";

if (channelId.Length == 0)
{
    Console.Error.WriteLine("Channel ID cannot be empty.");
    return;
}

await using HubConnection connection = new HubConnectionBuilder()
    .WithUrl($"{relayBaseUrl}/hubs/relay")
    .WithAutomaticReconnect()
    .Build();

connection.Reconnecting += exception =>
{
    Console.WriteLine();
    Console.WriteLine(
        $"Relay connection lost: {exception?.Message ?? "Unknown error"}");

    return Task.CompletedTask;
};

connection.Reconnected += async _ =>
{
    await connection.InvokeAsync("JoinChannel", channelId);

    Console.WriteLine();
    Console.WriteLine($"Reconnected to channel: {channelId}");
};

connection.Closed += exception =>
{
    Console.WriteLine();
    Console.WriteLine(
        $"Relay connection closed: {exception?.Message ?? "No error provided"}");

    return Task.CompletedTask;
};

try
{
    Console.WriteLine($"Connecting to: {relayBaseUrl}");

    await connection.StartAsync();
    await connection.InvokeAsync("JoinChannel", channelId);

    Console.WriteLine($"Connected to channel: {channelId}");
    Console.WriteLine("Enter a message or type /exit to close.");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Sender: ");

        string sender =
            Console.ReadLine()?.Trim()
            ?? string.Empty;

        if (string.Equals(
                sender,
                "/exit",
                StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (sender.Length == 0)
        {
            Console.WriteLine("Sender cannot be empty.");
            continue;
        }

        Console.Write("Message: ");

        string messageBody =
            Console.ReadLine()?.Trim()
            ?? string.Empty;

        if (messageBody.Length == 0)
        {
            Console.WriteLine("Message cannot be empty.");
            continue;
        }

        SmsPayload smsPayload = new()
        {
            Sender = sender,
            Body = messageBody,
            ReceivedAt = DateTimeOffset.Now
        };

        string serializedPayload =
            JsonSerializer.Serialize(smsPayload);

        RelayMessageEnvelope envelope = new()
        {
            ChannelId = channelId,
            Payload = serializedPayload,
            SentAt = DateTimeOffset.UtcNow
        };

        await connection.InvokeAsync(
            "SendMessage",
            envelope);

        Console.WriteLine("Message sent.");
        Console.WriteLine();
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"Simulator error: {exception.Message}");
}