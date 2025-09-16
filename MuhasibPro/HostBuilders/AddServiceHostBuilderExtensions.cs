using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using Muhasebe.Data.Database.SistemDatabase;
using MuhasibPro.Services.Common;
using MuhasibPro.ViewModels.Contracts.Common;

namespace MuhasibPro.HostBuilders
{
    public static class AddServiceHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddScoped<StartupSistemDatabase>();
                //Uygulama içi service
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
