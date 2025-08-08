using Microsoft.Extensions.Configuration;

namespace Muhasebe.Data.DataContext;

public static class ConfigurationDb
{
    public static IConfigurationRoot GetConfiguration()
    {
        var dbPath = AppContext.BaseDirectory;
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        return builder.Build();
    }
}