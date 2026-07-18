namespace SmsBridge.Mobile.Services;

public interface IIncomingMessageForwarder
{
    Task ForwardAsync(
        string sender,
        string body,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken = default);
}