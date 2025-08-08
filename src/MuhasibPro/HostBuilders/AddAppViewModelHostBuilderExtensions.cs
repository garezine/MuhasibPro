using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.ViewModels.ViewModel;
using MuhasibPro.ViewModels.ViewModel.CalismaDonem;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.ViewModels.ViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModel.Login;
using MuhasibPro.ViewModels.ViewModel.Shell;

namespace MuhasibPro.HostBuilders;
public static  class AddAppViewModelHostBuilderExtensions
{
    public static IHostBuilder AddAppViewModel(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {            
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainShellViewModel>();
            services.AddTransient<SettingsViewModel>();
            

            services.AddTransient<FirmalarViewModel>();
            services.AddTransient<FirmaDetailsViewModel>();

            services.AddTransient<CalismaDonemDetailsViewModel>();

        });
        return host;
    }

}
