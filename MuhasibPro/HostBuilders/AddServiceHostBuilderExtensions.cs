using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MuhasibPro.Contracts.SistemServices;
using MuhasibPro.Services.SistemServices.FirmaMaliDonemService;

namespace MuhasibPro.HostBuilders
{
    public static class AddServiceHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {

                //Uygulama içi service
               

                //services.AddScoped<ITenantManagementService, TenantManagementService>();

                services.AddScoped<IFirmaService, FirmaService>();
                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
