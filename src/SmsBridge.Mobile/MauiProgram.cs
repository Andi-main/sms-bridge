using Microsoft.Extensions.Logging;
using SmsBridge.Mobile.Configuration;
using SmsBridge.Mobile.Services;
#if ANDROID
using SmsBridge.Mobile.Platforms.Android.Services;
#endif

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

        builder.Services.AddSingleton<
    IIncomingMessageForwarder,
    IncomingMessageForwarder>();

    #if ANDROID
            builder.Services.AddSingleton<
                ISmsPermissionService,
                AndroidSmsPermissionService>();
    #endif

        return builder.Build();
    }
}
