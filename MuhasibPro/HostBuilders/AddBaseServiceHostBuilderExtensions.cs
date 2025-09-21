using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using MuhasibPro.Services.BaseApp;
using MuhasibPro.Services.Common;
using MuhasibPro.Tools;
using MuhasibPro.ViewModels.Contracts.BaseApp;
using MuhasibPro.ViewModels.Contracts.Common;

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



                //services.AddScoped<ITenantManagementService, TenantManagementService>();


                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
