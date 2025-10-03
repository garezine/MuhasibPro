using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using MuhasibPro.Contracts.BaseAppServices;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;
using MuhasibPro.Services.BaseServices;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.Services.SistemServices.DatabaseServices;
using MuhasibPro.Tools;

namespace MuhasibPro.HostBuilders
{
    public static class AddBaseServiceHostBuilderExtensions
    {
        public static IHostBuilder AddBaseServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                //Common services
                services.AddScoped<IBitmapTools, BitmapTools>();
                services.AddScoped<IFilePickerService, FilePickerService>();
                services.AddScoped<IFileService, FileService>();
                services.AddScoped<IContextService, ContextService>();
                services.AddScoped<INavigationService, NavigationService>();
                services.AddScoped<IMessageService, MessageService>();
                services.AddScoped<IDialogService, DialogService>();

                services.AddScoped<INotificationService, NotificationService>();

                services.AddScoped<ICommonServices, CommonServices>();               

                //LogServices
                services.AddScoped<ISistemLogService, SistemLogService>();
                services.AddScoped<IAppLogService, AppLogService>();
                services.AddScoped<ILogService, LogService>();

                //Application services
                services.AddScoped<IActivationService, ActivationService>();
                services.AddScoped<ILocalSettingsService, LocalSettingsService>();
                services.AddScoped<IThemeSelectorService, ThemeSelectorService>();
                services.AddScoped<IWebViewService, WebViewService>();

                services.AddScoped<ISistemDatabaseService, SistemDatabaseService>();
                services.AddScoped<IAppDatabaseService, AppDatabaseService>();
                services.AddScoped<IDatabaseUpdateService, DatabaseUpdateService>();
                services.AddScoped<IUpdateService, UpdateService>();
                services.AddScoped<IAuthenticationService, AuthenticationService>();


            });
            return host;
        }
    }
}
