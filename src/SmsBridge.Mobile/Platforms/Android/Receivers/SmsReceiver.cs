using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

using SmsBridge.Mobile.Services;

namespace SmsBridge.Mobile.Platforms.Android.Receivers;

[global::Android.Content.BroadcastReceiver(
    Enabled = true,
    Exported = true,
    Permission =
        global::Android.Manifest.Permission.BroadcastSms)]
[global::Android.App.IntentFilter(
    new[]
    {
        global::Android.Provider.Telephony
            .Sms.Intents.SmsReceivedAction
    })]
public sealed class SmsReceiver
    : global::Android.Content.BroadcastReceiver
{
    public override void OnReceive(
        global::Android.Content.Context? context,
        global::Android.Content.Intent? intent)
    {
        if (intent?.Action !=
            global::Android.Provider.Telephony
                .Sms.Intents.SmsReceivedAction)
        {
            return;
        }

        PendingResult? pendingResult = GoAsync();

        if (pendingResult is null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            ILogger<SmsReceiver>? logger = null;

            try
            {
                IServiceProvider? services =
                    IPlatformApplication.Current?.Services;

                if (services is null)
                {
                    return;
                }

                logger = services.GetService<
                    ILogger<SmsReceiver>>();

                global::Android.Telephony.SmsMessage[]?
                    messages =
                        global::Android.Provider.Telephony
                            .Sms.Intents
                            .GetMessagesFromIntent(intent);

                if (messages is null ||
                    messages.Length == 0)
                {
                    return;
                }

                global::Android.Telephony.SmsMessage
                    firstMessage = messages[0];

                string sender =
                    firstMessage.OriginatingAddress?.Trim()
                    ?? "Unknown sender";

                string body = string.Concat(
                    messages.Select(message =>
                        message.MessageBody
                        ?? string.Empty));

                if (string.IsNullOrWhiteSpace(body))
                {
                    return;
                }

                DateTimeOffset receivedAt =
                    DateTimeOffset
                        .FromUnixTimeMilliseconds(
                            firstMessage.TimestampMillis);

                IIncomingMessageForwarder forwarder =
                    services.GetRequiredService<
                        IIncomingMessageForwarder>();

                await forwarder.ForwardAsync(
                    sender,
                    body,
                    receivedAt);
            }
            catch (Exception exception)
            {
                logger?.LogError(
                    exception,
                    "Could not forward incoming SMS.");
            }
            finally
            {
                pendingResult.Finish();
            }
        });
    }
}