using Microsoft.Extensions.Logging;
using SmsBridge.Mobile.Configuration;
using SmsBridge.Mobile.Services;

namespace SmsBridge.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        const string relayBaseUrl = "http://localhost:5111";

        builder.Services.AddSingleton(new RelayOptions
        {
            BaseUrl = relayBaseUrl,
            ChannelId = "demo-channel"
        });

        builder.Services.AddSingleton<RelayConnectionService>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
