using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstract.App;
using MuhasibPro.App.Services;
using MuhasibPro.App.Services.Common;
using MuhasibPro.Core.Models;
using MuhasibPro.Core.Services;
using MuhasibPro.Core.Services.Abstract.Common;
using MuhasibPro.Core.Services.Concreate.App;
using MuhasibPro.Core.Services.Concreate.Update;

namespace MuhasibPro.App.HostBuilders;
public static class AddAppServiceHostBuilderExtensions
{
    public static IHostBuilder AddAppServices(this IHostBuilder host)
    {
        host.ConfigureServices((context, services) =>
        {
            //services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();


            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddSingleton<IFileService, FileService>();


            services.AddSingleton<ICommonServices, CommonServices>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddScoped<IContextService, ContextService>();

            services.AddSingleton<IUpdateService, UpdateService>();


            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));

            services.AddScoped<IFirmaService, FirmaService>();

        });
        return host;
    }

}
