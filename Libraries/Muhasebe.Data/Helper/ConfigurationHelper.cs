using Microsoft.Extensions.Configuration;

namespace Muhasebe.Data.Helper
{
    public class ConfigurationHelper
    {
        private static readonly Lazy<ConfigurationHelper> lazy = new(() => new ConfigurationHelper());
        public static ConfigurationHelper Instance => lazy.Value;
        private ConfigurationHelper() { }

        public IConfigurationRoot GetConfiguration()
        {
            var dbPath = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
        public string GetProjectPath()
        {
            // Çalışan exe'nin bulunduğu dizinden başla
            var currentDirectory = AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(currentDirectory);

            // bin klasöründen üst dizinlere doğru çık
            while (directoryInfo != null)
            {
                // .csproj dosyası var mı kontrol et
                var projectFiles = directoryInfo.GetFiles("*.csproj");
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
                    if (dir.GetFiles("*.csproj").Length > 0)
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

}
