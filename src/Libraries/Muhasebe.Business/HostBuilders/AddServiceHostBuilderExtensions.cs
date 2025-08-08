using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Business.Services.Concreate.Common;

namespace Muhasebe.Business.HostBuilders
{
    public static class AddServiceHostBuilderExtensions
    {
        public static IHostBuilder AddServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddScoped<ILogService, LogService>();
                services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                //services.AddScoped<ITenantManagementService, TenantManagementService>();


                //services.AddScoped<ICalismaDonemService, CalismaDonemService>();
                //services.AddScoped<ICalismaDonemDbService, CalismaDonemDbService>();
            });
            return host;
        }
    }
}
