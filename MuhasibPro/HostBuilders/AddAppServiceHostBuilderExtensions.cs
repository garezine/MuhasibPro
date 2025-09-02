using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstract.App;
using MuhasibPro.Services;
using MuhasibPro.Services.Common;
using MuhasibPro.Infrastructure.Models;
using MuhasibPro.Infrastructure.Services;
using MuhasibPro.Infrastructure.Services.Abstract.Common;
using MuhasibPro.Infrastructure.Services.Concreate.App;
using MuhasibPro.Infrastructure.Services.Concreate.Update;

namespace MuhasibPro.HostBuilders;
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
