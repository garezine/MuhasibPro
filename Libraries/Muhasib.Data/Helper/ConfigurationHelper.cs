using Microsoft.Extensions.Configuration;

namespace Muhasib.Data.Helper
{
    public class ConfigurationHelper
    {
        public const string DatabaseKlasorAdi = "Databases";
        public const string SistemDbAdi = "Sistem.db";
        public const string YedekKlasorAdi = "Backups";
        public const string GeciciKlasorAdi = "Temp";

        public string MuhasebeVeritabaniAdi(string firmaKodu, int maliDonemYil)
        {
            var timestamp = DateTime.Now.ToString("MMddHHmm"); // ✅ TIMESTAMP EKLE
            return $"Muhasebe_{firmaKodu}_{maliDonemYil}_{timestamp}";
        }

        public string YedekDosyaAdi(string firmaKodu)
        { return $"{firmaKodu}_Yedek_{DateTime.Now:yyyyMMdd_HHmmss}.bak"; }

        private static readonly Lazy<ConfigurationHelper> lazy = new(() => new ConfigurationHelper());

        public static ConfigurationHelper Instance => lazy.Value;

        private ConfigurationHelper()
        {
        }

        public IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Executable'ın bulunduğu yer
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        public string GetProjectPath()
        {
            // Development ortamında (Debug/Release klasörü varsa)
            if (IsDebugEnvironment())
            {
                return GetDevelopmentProjectPath();
            }

            // Production ortamında - AppData kullan
            return GetAppDataPath();
        }

        /// <summary>
        /// Uygulama verilerinin saklanacağı klasör yolu
        /// </summary>
        public string GetAppDataPath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MuhasibPro" // Uygulamanızın adı
            );
            // Klasör yoksa oluştur
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
                Console.WriteLine($"AppData klasörü oluşturuldu: {appDataPath}");
            }

            return appDataPath;
        }

        /// <summary>
        /// Development ortamında mı çalışıyor kontrolü
        /// </summary>
        public bool IsDebugEnvironment()
        {
            var currentDir = AppContext.BaseDirectory;

            // bin klasörü içinde olup Debug veya Release içeriyorsa development
            return currentDir.Contains("\\bin\\") && (currentDir.Contains("Debug") || currentDir.Contains("Release"));
        }

        /// <summary>
        /// Development ortamında proje klasörünü bul
        /// </summary>
        private string GetDevelopmentProjectPath()
        {
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

            // Alternatif yöntem
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

            // Son çare: AppData kullan
            return GetAppDataPath();
        }

        /// <summary>
        /// Database klasörü yolu (hem development hem production için)
        /// </summary>
        public string GetDatabasePath()
        {
            var basePath = GetProjectPath();
            var dbPath = Path.Combine(basePath, DatabaseKlasorAdi);

            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
                Console.WriteLine($"Database klasörü oluşturuldu: {dbPath}");
            }

            return dbPath;
        }

        public void TestPaths()
        {
            System.Diagnostics.Debug.WriteLine("=== PATH DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");
            System.Diagnostics.Debug.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            System.Diagnostics.Debug
                .WriteLine($"Assembly Location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");

            var currentDir = AppContext.BaseDirectory;
            System.Diagnostics.Debug.WriteLine($"Current Dir: {currentDir}");
            System.Diagnostics.Debug.WriteLine($"Contains \\bin\\Debug: {currentDir.Contains("\\bin\\Debug")}");
            System.Diagnostics.Debug.WriteLine($"Contains \\bin\\Release: {currentDir.Contains("\\bin\\Release")}");

            System.Diagnostics.Debug.WriteLine($"IsDebugEnvironment(): {IsDebugEnvironment()}");

            if (IsDebugEnvironment())
            {
                System.Diagnostics.Debug.WriteLine($"GetDevelopmentProjectPath(): {GetDevelopmentProjectPath()}");
            }

            System.Diagnostics.Debug.WriteLine($"GetAppDataPath(): {GetAppDataPath()}");
            System.Diagnostics.Debug.WriteLine($"Final GetProjectPath(): {GetProjectPath()}");
            System.Diagnostics.Debug.WriteLine($"GetDatabasePath(): {GetDatabasePath()}");
            System.Diagnostics.Debug.WriteLine("=== END PATH DEBUG ===");
        }
    }
}