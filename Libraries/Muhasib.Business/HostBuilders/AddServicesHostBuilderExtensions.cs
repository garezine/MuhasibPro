using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Business.Services.Concrete.AppServices;
using Muhasib.Business.Services.Concrete.BaseServices;
using Muhasib.Business.Services.Concrete.SistemServices;
using Muhasib.Business.Services.Contracts.AppServices;
using Muhasib.Business.Services.Contracts.BaseServices;

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
                services.AddSingleton<IFirmaWithMaliDonemSelectedService, FirmaWithMaliDonemSelectedService>();
                
            });
            return host;
        }
    }
}
