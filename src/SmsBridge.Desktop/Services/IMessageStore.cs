using SmsBridge.Desktop.Models;

namespace SmsBridge.Desktop.Services;

public interface IMessageStore
{
    event Action<SmsMessage> MessageReceived;
    IReadOnlyList<SmsMessage> GetMessages();

    void AddMessage(SmsMessage message);
}
