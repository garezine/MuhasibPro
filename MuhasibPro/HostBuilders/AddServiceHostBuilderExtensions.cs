using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.Services.SistemServices.DatabaseServices;
using MuhasibPro.Services.SistemServices.FirmaMaliDonemService;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.Contracts.SistemServices;
using MuhasibPro.ViewModels.Contracts.SistemServices.DatabaseServices;

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

                services.AddScoped<IFirmaService,FirmaService>();
                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
