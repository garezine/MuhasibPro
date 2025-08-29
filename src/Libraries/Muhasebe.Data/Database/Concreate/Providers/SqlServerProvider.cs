using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Providers
{
    public class SqlServerProvider : IDatabaseProvider
    {
        public DatabaseType DbType => DatabaseType.SqlServer;

        private readonly IConfiguration _config; // Master DB'ye bağlanmak için gerekebilir
        private readonly ILogger<SqlServerProvider> _logger; // Logger ekle

        public SqlServerProvider(IConfiguration config, ILogger<SqlServerProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        private async Task<bool> IsLocalSqlServerInstalledAsync()
        {
            string[] commonLocalServers = { ".\\SQLEXPRESS", "localhost\\SQLEXPRESS", "(localdb)\\MSSQLLocalDB", "." };

            foreach (var server in commonLocalServers)
            {
                try
                {
                    var testConnectionString = new SqlConnectionStringBuilder
                    {
                        DataSource = server,
                        IntegratedSecurity = true, // Windows Authentication
                        TrustServerCertificate = true,
                        ConnectTimeout = 2 // 2 saniye timeout
                    }.ConnectionString;

                    using (var connection = new SqlConnection(testConnectionString))
                    {
                        await connection.OpenAsync(); // Bağlantıyı dene
                        await connection.CloseAsync();

                        _logger.LogInformation("SQL Server bulundu: {Server}", server);
                        return true;
                    }
                }
                catch (SqlException ex)
                {
                    _logger.LogDebug("SQL Server {Server} bağlantı hatası: {Error}", server, ex.Message);
                }
            }

            _logger.LogError("Yerel SQL Server örnekleri bulunamadı veya erişilemiyor!");
            return false;
        }

        public string GenerateConnectionString(string dbName, string baseDir)
        {
            var dataSource = _config["DatabaseDefaults:DataSource"] ?? "(LocalDB)\\MSSQLLocalDB";
            var userId = _config["DatabaseDefaults:UserId"];
            var password = _config["DatabaseDefaults:Password"];
            var trustServerCertificate = bool.TryParse(_config["DataDefaults:TrustServerCertificate"], out var trust) &&
                trust;
            var integratedSecurity = bool.TryParse(_config["DatabaseDefaults:Integrated Security"], out var result) &&
                result;

            // Bağlantı dizesini oluştur
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = dbName,
                IntegratedSecurity = integratedSecurity,
                TrustServerCertificate = trustServerCertificate,
            };

            if (!builder.IntegratedSecurity)
            {
                builder.UserID = userId;
                builder.Password = password;
            }
            if (!string.IsNullOrEmpty(baseDir))
            {
                builder.AttachDBFilename = Path.Combine(baseDir, $"{dbName}.mdf");
            }

            return builder.ConnectionString;
        }

        public void ConfigureContext(DbContextOptionsBuilder options, string connectionString) => options.UseSqlServer(
            connectionString);

        public async Task CreateDatabaseAsync(string dbPathOrIdentifier, string baseDir) // Burada dbPathOrIdentifier = dbName
        {
            var isInstalled = await IsLocalSqlServerInstalledAsync();
            if (!isInstalled)
            {
                throw new Exception("SQL Server kurulu değil veya erişilemiyor!");
            }

            string dbName = dbPathOrIdentifier;
            _logger.LogInformation("Attempting to create SQL Server database: {DbName}", dbName);

            // Master veritabanına bağlanmak için connection string
            // AppSettings'den veya başka bir konfigürasyondan alınabilir.
            // ÖNEMLİ: Master connection string'i güvenli bir şekilde yönetin.
            var masterConnectionString = GenerateConnectionString(dbName: "master", baseDir: null); // Varsayılan connection string değil!
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new InvalidOperationException("MasterConnection connection string is not configured.");
            }

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(masterConnectionString);

            // Geçici bir context ile master'a bağlan
            await using var masterContext = new DbContext(optionsBuilder.Options); // Basit DbContext yeterli
            try
            {
                // Veritabanını oluşturma komutu
                // Parametre kullanarak SQL Injection'a karşı koruma sağlayın.
                var commandText = $@"
CREATE DATABASE [{dbName}]
ON PRIMARY (
    NAME = '{dbName}_Data',
    FILENAME = '{Path.Combine(baseDir, $"{dbName}.mdf")}'
)
LOG ON (
    NAME = '{dbName}_Log',
    FILENAME = '{Path.Combine(baseDir, $"{dbName}_log.ldf")}'
)"; // Köşeli parantez isimdeki özel karakterlere izin verir.
                await masterContext.Database.ExecuteSqlRawAsync(commandText).ConfigureAwait(false);
                _logger.LogInformation("SQL Server database {DbName} created successfully.", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SQL Server database {DbName}.", dbName);
                throw; // Hata tekrar fırlatılır.
            }
        }


        public async Task ApplySchemaAsync(DbContext context)
        {
            _logger.LogInformation("Applying schema (Migrate) for SQL Server.");
            // Genellikle Migration kullanılır. EnsureCreated test/demo için olabilir.
            // await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            await context.Database.MigrateAsync().ConfigureAwait(false); // Önerilen
        }

        public async Task CleanupDatabaseAsync(string dbPathOrIdentifier, string baseDir)
        {
            string dbName = dbPathOrIdentifier;
            _logger.LogWarning("Attempting to cleanup SQL Server database: {DbName}", dbName);
            if (!IsValidDatabaseName(dbName))
            {
                _logger.LogError("Invalid database name: {DbName}. Potential SQL injection attempt.", dbName);
                throw new ArgumentException("Invalid database name.");
            }
            var masterConnectionString = GenerateConnectionString(dbName: "master", baseDir: null); ;
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                _logger.LogError("MasterConnection connection string is not configured.");
                throw new InvalidOperationException("MasterConnection string is missing!");
            }

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(masterConnectionString, options => options.CommandTimeout(60));

            await using var masterContext = new DbContext(optionsBuilder.Options);
            try
            {
                // Dinamik SQL oluşturma (dbName parametreleştirilemez, bu yüzden manuel sanitize edildi)
                var commandText = $@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName.Replace("'", "''")}')
            BEGIN
                ALTER DATABASE [{dbName.Replace("]", "]]")}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{dbName.Replace("]", "]]")}];
            END";

                await masterContext.Database.ExecuteSqlRawAsync(commandText);
                _logger.LogInformation("SQL Server database {DbName} dropped successfully.", dbName);
                var mdfPath = Path.Combine(baseDir, $"{dbName}.mdf");
                if (File.Exists(mdfPath))
                {
                    File.Delete(mdfPath);
                }
            }
            catch (SqlException ex) when (ex.Number == 3701) // Database does not exist
            {
                _logger.LogWarning("Database {DbName} does not exist.", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop SQL Server database {DbName}.", dbName);
                throw;
            }
        }
        private bool IsValidDatabaseName(string dbName)
        {
            return !string.IsNullOrWhiteSpace(dbName)
                   && dbName.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}



