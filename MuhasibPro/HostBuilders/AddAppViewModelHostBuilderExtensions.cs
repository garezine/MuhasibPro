using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.Common;
using MuhasibPro.ViewModels.ViewModels.CalismaDonem;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Firmalar;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;

namespace MuhasibPro.HostBuilders;
public static class AddAppViewModelHostBuilderExtensions
{
    public static IHostBuilder AddAppViewModel(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddTransient<IValidationHelper, ValidationHelper>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainShellViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UpdateViewModel>();


            services.AddTransient<FirmalarViewModel>();
            services.AddTransient<FirmaDetailsViewModel>();

            services.AddTransient<CalismaDonemDetailsViewModel>();

        });
        return host;
    }

}
