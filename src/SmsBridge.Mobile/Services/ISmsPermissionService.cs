namespace SmsBridge.Mobile.Services;

public interface ISmsPermissionService
{
    Task<bool> IsGrantedAsync();

    Task<bool> RequestAsync();
}