#if ANDROID

using Microsoft.Maui.ApplicationModel;

namespace SmsBridge.Mobile.Platforms.Android.Services;

public sealed class ReceiveSmsPermission
    : Permissions.BasePlatformPermission
{
    public override (
        string androidPermission,
        bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
            (
                global::Android.Manifest.Permission.ReceiveSms,
                true
            )
        }.ToArray();
}

#endif