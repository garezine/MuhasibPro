using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Business.Services.Concrete.BaseServices;
using Muhasib.Business.Services.Concrete.SistemServices;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.SistemServices;

namespace Muhasib.Business.HostBuilders
{
    public static class AddServicesHostBuilderExtensions
    {
        public static IHostBuilder AddBusinessServices(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddSingleton<IAuthenticationService, AuthenticationService>();
                services.AddSingleton<IFirmaService, FirmaService>();
                services.AddSingleton<IMaliDonemService, MaliDonemService>();
                
            });
            return host;
        }
    }
}
