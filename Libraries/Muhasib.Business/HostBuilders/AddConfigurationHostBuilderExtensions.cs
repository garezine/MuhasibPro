using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muhasib.Business.Infrastructure.Provider;

namespace Muhasib.Business.HostBuilders
{
    public static class AddConfigurationHostBuilderExtensions
    {
        public static IHostBuilder AddConfiguration(this IHostBuilder host)
        {
            host.ConfigureHostConfiguration(
                c =>
                {
                    c.AddJsonFile("appsettings.json");
                });
            host.ConfigureLogging((context, logging) =>
            {
                // Console ve Debug logger'ları
                logging.AddConsole();
                logging.AddDebug();

                // Basit file logging
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                Directory.CreateDirectory(logPath);
                var logFile = Path.Combine(logPath, $"muhasib-{DateTime.Now:yyyyMMdd}.txt");

                logging.AddProvider(new SimpleFileLoggerProvider(logFile));
            });

            return host;
        }
    }
}
