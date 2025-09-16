using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MuhasibPro.HostBuilders
{
    public static class AddSystemLogManagerHostBuilderExtensions
    {
        public static IHostBuilder AddSystemLog(this IHostBuilder host)
        {
            host.ConfigureLogging(logging =>
            {
                logging.AddDebug();
                logging.AddConsole();
            });
            return host;
        }
    }
}
