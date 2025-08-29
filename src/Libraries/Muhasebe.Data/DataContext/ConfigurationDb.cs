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
    public static string GetProjectDirectory()
    {
        // Çalışan exe'nin bulunduğu dizinden başla
        var currentDirectory = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(currentDirectory);

        // bin klasöründen üst dizinlere doğru çık
        while (directoryInfo != null)
        {
            // .csproj dosyası var mı kontrol et
            var projectFiles = directoryInfo.GetFiles("*.wapproj");
            if (projectFiles.Length > 0)
            {
                return directoryInfo.FullName;
            }

            directoryInfo = directoryInfo.Parent;
        }

        // Eğer proje dosyası bulunamazsa, alternatif yöntem
        // Bu genellikle development ortamında çalışır
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        if (assemblyDir != null)
        {
            var dir = new DirectoryInfo(assemblyDir);
            while (dir != null)
            {
                if (dir.GetFiles("*.wapproj").Length > 0)
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
        }

        // Son çare olarak current directory'yi döndür
        return Environment.CurrentDirectory;
    }

}