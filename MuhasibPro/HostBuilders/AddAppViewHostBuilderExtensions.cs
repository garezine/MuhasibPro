using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Views.Dashboard;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Login;
using MuhasibPro.Views.Settings;
using MuhasibPro.Views.Shell;

namespace MuhasibPro.HostBuilders;
public static class AddAppViewHostBuilderExtensions
{
    public static IHostBuilder AddAppView(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddTransient<ShellView>();
            services.AddTransient<LoginView>();
            services.AddTransient<MainShellView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<FirmaView>();
            services.AddTransient<UpdateView>();
            services.AddTransient<SettingsView>();
        });
        return host;
    }
}
