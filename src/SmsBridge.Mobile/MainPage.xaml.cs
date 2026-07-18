using SmsBridge.Mobile.Services;

namespace SmsBridge.Mobile;

public partial class MainPage : ContentPage
{
    private readonly RelayConnectionService _relayConnection;

    public MainPage(
    RelayConnectionService relayConnection,
    ISmsPermissionService smsPermissionService)
    {
        InitializeComponent();

        _relayConnection = relayConnection;
        _smsPermissionService = smsPermissionService;

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

    private readonly ISmsPermissionService
    _smsPermissionService;

    private async void OnEnableSmsClicked(
    object? sender,
    EventArgs eventArgs)
    {
        try
        {
            EnableSmsButton.IsEnabled = false;

            bool granted =
                await _smsPermissionService.RequestAsync();

            SmsPermissionLabel.Text = granted
                ? "SMS forwarding enabled"
                : "SMS permission denied";
        }
        catch (Exception exception)
        {
            SmsPermissionLabel.Text =
                $"Permission request failed: {exception.Message}";
        }
        finally
        {
            EnableSmsButton.IsEnabled = true;
        }
    }
}