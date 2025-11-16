using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Muhasib.Business.HostBuilders
{
    public static class AddConfigurationHostBuilderExtensions
    {
        public static IHostBuilder AddConfiguration(this IHostBuilder host)
        {
            host.ConfigureHostConfiguration(c =>
            {
                c.AddJsonFile("appsettings.json");
            });
            return host;
        }
    }
}
