using SmsBridge.Mobile.Services;

namespace SmsBridge.Mobile;

public partial class MainPage : ContentPage
{
    private readonly RelayConnectionService _relayConnection;

    public MainPage(RelayConnectionService relayConnection)
    {
        InitializeComponent();

        _relayConnection = relayConnection;
        _relayConnection.ConnectionStatusChanged +=
            HandleConnectionStatusChanged;
    }

    private async void OnConnectClicked(
        object? sender,
        EventArgs eventArgs)
    {
        try
        {
            ConnectButton.IsEnabled = false;
            ResultLabel.Text = string.Empty;

            await _relayConnection.ConnectAsync();

            SendButton.IsEnabled =
                _relayConnection.IsConnected;
        }
        catch (Exception exception)
        {
            ResultLabel.Text =
                $"Connection failed: {exception.Message}";

            ConnectButton.IsEnabled = true;
        }
    }

    private async void OnSendClicked(
    object? sender,
    EventArgs eventArgs)
    {
        try
        {
            SendButton.IsEnabled = false;
            ResultLabel.Text = string.Empty;

            await _relayConnection.SendMessageAsync(
                SenderEntry.Text ?? string.Empty,
                MessageEditor.Text ?? string.Empty);

            ResultLabel.Text = "Message sent.";
            MessageEditor.Text = string.Empty;
        }
        catch (Exception exception)
        {
            ResultLabel.Text =
                $"Sending failed: {exception.Message}";
        }
        finally
        {
            SendButton.IsEnabled =
                _relayConnection.IsConnected;
        }
    }

    private void HandleConnectionStatusChanged(string status)
    {
        Dispatcher.Dispatch(() =>
        {
            StatusLabel.Text = status;

            bool isConnected =
                _relayConnection.IsConnected;

            ConnectButton.IsEnabled = !isConnected;
            SendButton.IsEnabled = isConnected;
        });
    }
}