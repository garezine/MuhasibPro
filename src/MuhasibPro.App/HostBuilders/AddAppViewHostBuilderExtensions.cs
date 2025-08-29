using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.App.Views.Dashboard;
using MuhasibPro.App.Views.Firma;
using MuhasibPro.App.Views.Login;
using MuhasibPro.App.Views.Settings;
using MuhasibPro.App.Views.Shell;

namespace MuhasibPro.App.HostBuilders;
public static  class AddAppViewHostBuilderExtensions
{
    public static IHostBuilder AddAppView(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {            
            services.AddTransient<LoginView>();
            services.AddTransient<ShellView>();
            services.AddTransient<MainShellView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<FirmaView>();
            services.AddTransient<UpdateView>();
            

        });
        return host;
    }
}
