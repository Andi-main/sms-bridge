using SmsBridge.Desktop.Models;
namespace SmsBridge.Desktop.Services;

public sealed class InMemoryMessageStore : IMessageStore
{
    private readonly List<SmsMessage> _messages =
    [
        new SmsMessage{
        Sender = "Mama",
        Body = "Kup proszę mleko po drodze.",
        ReceivedAt = DateTime.Today.AddHours(13).AddMinutes(12)
        },
        new SmsMessage{
        Sender = "+48 123 456 789",
        Body = "Twój kod odbioru przesyłki to: xyz",
        ReceivedAt = DateTime.Today.AddHours(11).AddMinutes(10)
        },
        new SmsMessage{
        Sender = "Wiktoria",
        Body = "Jestem Góralcio :3",
        ReceivedAt = DateTime.Today.AddHours(8).AddMinutes(37)
        },
    ];
    public IReadOnlyList<SmsMessage> GetMessages()
    {
        return _messages;
    }
}
