using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

namespace MuhasibPro.HostBuilders;
public static class AddAppViewModelHostBuilderExtensions
{
    public static IHostBuilder AddAppViewModel(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainShellViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UpdateViewModel>();


            services.AddTransient<FirmalarViewModel>();
            services.AddTransient<FirmaDetailsViewModel>();
            services.AddTransient<FirmaDetailsWithMaliDonemlerViewModel>();

            services.AddTransient<MaliDonemViewModel>();
            services.AddTransient<MaliDonemDetailsViewModel>();

            services.AddTransient<SistemLogsViewModel>();

        });
        return host;
    }

}
