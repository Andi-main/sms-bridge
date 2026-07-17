using SmsBridge.Desktop.Models;

namespace SmsBridge.Desktop.Services;

public interface IMessageStore
{
    IReadOnlyList<SmsMessage> GetMessages();
}
