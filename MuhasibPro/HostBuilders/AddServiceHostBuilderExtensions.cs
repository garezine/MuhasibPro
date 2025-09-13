using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using MuhasibPro.Services.Common;

namespace MuhasibPro.HostBuilders
{
    public static class AddServiceHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddScoped<ISistemLogService, SistemLogService>();
                services.AddScoped<IAppLogService, AppLogService>();
                services.AddScoped<ILogService, LogService>();
                
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                

                //services.AddScoped<ITenantManagementService, TenantManagementService>();


                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
