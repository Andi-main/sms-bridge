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

        #if DEBUG
		    builder.Logging.AddDebug();
        #endif
        #if ANDROID
            const string relayBaseUrl = "http://10.0.2.2:5111";
        #else
            const string relayBaseUrl = "http://localhost:5111";
        #endif

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
