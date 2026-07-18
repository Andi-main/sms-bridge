#if ANDROID

using Microsoft.Maui.ApplicationModel;

using SmsBridge.Mobile.Services;

namespace SmsBridge.Mobile.Platforms.Android.Services;

public sealed class AndroidSmsPermissionService
    : ISmsPermissionService
{
    public async Task<bool> IsGrantedAsync()
    {
        PermissionStatus status =
            await Permissions.CheckStatusAsync<
                ReceiveSmsPermission>();

        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestAsync()
    {
        PermissionStatus status =
            await Permissions.RequestAsync<
                ReceiveSmsPermission>();

        return status == PermissionStatus.Granted;
    }
}

#endif