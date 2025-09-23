using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using MuhasibPro.Services.CommonServices;
using MuhasibPro.Services.SistemServices.DatabaseServices;
using MuhasibPro.ViewModels.Contracts.CommonServices;
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
                services.AddScoped<ISistemDatabaseService, SistemDatabaseService>();
                services.AddScoped<IAppDatabaseService, AppDatabaseService>();
                services.AddScoped<IDatabaseUpdateService, DatabaseUpdateService>();
                services.AddScoped<IUpdateService, UpdateService>();
                services.AddScoped<IAuthenticationService, AuthenticationService>();


                //services.AddScoped<ITenantManagementService, TenantManagementService>();


                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
