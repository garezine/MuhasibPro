using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Business.Services.Contracts.UIServices;
using Muhasib.Business.Services.Contracts.UtilityServices;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.Services.BaseServices;
using MuhasibPro.Services.Infrastructure.CommonServices;
using MuhasibPro.Services.Infrastructure.LogServices;
using MuhasibPro.Services.Infrastructure.UtilityService;

namespace MuhasibPro.HostBuilders
{
    public static class AddCommonServiceHostBuilderExtensions
    {
        public static IHostBuilder AddCommonServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                // ✅ GLOBAL & STATEFUL servisler - SINGLETON:
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();
                services.AddSingleton<ISistemLogService, SistemLogService>();
                services.AddSingleton<IAppLogService, AppLogService>();
                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IMessageService, MessageService>();



                // ✅ VIEW/OPERATION başına - SCOPED:
                services.AddScoped<IStatusBarService, StatusBarService>();
                services.AddScoped<IStatusMessageService, StatusMessageService>();
                services.AddScoped<IBitmapToolsService, BitmapToolsService>();
                services.AddScoped<IWebViewService, WebViewService>();


                services.AddScoped<ICommonServices, CommonServices>();
                services.AddScoped<IContextService, ContextService>();
                services.AddScoped<INavigationService, NavigationService>();





            });
            return host;
        }
    }
}
