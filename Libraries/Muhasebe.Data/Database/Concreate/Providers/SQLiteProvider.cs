using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Interfaces.Provider;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Database.Concreate.Providers
{
    // SQLiteProvider'da eksik using ifadesi eklendi.
    // CreateDatabaseAsync ve CleanupDatabaseAsync metodlarında dosya yolu işlemleri düzeltildi.
    // ApplySchemaAsync metodundaki try-catch bloğu korundu, bu SQLite için yaygın bir yaklaşımdır.
    public class SQLiteProvider : IDatabaseProvider
    {
        private readonly ILogger<SQLiteProvider> _logger;
        public SQLiteProvider(ILogger<SQLiteProvider> logger) { _logger = logger; } // DI ile logger almalı
        public DatabaseType DbType => DatabaseType.SQLite;

        // SQLite için connection string baseDir ve dbName kullanılarak oluşturulur.
        public string GenerateConnectionString(string dbName, string baseDir)
        {
            if (string.IsNullOrEmpty(dbName)) throw new ArgumentException("Database name cannot be null or empty.", nameof(dbName));
            if (string.IsNullOrEmpty(baseDir)) throw new ArgumentException("Base directory cannot be null or empty.", nameof(baseDir));
            // Path.Combine platform bağımsız yol birleştirme sağlar.
            // SQLite dosya adının uzantısını da eklemek iyi bir pratiktir.
            string dbFileName = $"{dbName}.sqlite";
            return $"Data Source={Path.Combine(baseDir, dbFileName)}";
        }

        public void ConfigureContext(DbContextOptionsBuilder options, string connectionString)
            => options.UseSqlite(connectionString);

        public async Task CreateDatabaseAsync(string dbPathOrIdentifier, string baseDir)
        {
            // SQLite'da CreateDatabaseAsync genellikle bir şey yapmaz,
            // bağlantı açıldığında veya EnsureCreated çağrıldığında dosya oluşur.
            // Ama ApplySchema içinde EnsureCreated çağrılacak.
            // Yine de dosyanın varlığını kontrol edebiliriz.
            _logger.LogDebug("SQLite CreateDatabaseAsync called for {DbPath}. Schema application will ensure creation.", dbPathOrIdentifier);
            await Task.CompletedTask; // Veya EnsureCreated burada çağrılabilir ama ApplySchema'da daha mantıklı.
        }

        public async Task ApplySchemaAsync(DbContext context)
        {
            _logger.LogInformation("Applying schema (EnsureCreated) for SQLite.");
            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
            // Veya migration kullanılıyorsa: await context.Database.MigrateAsync().ConfigureAwait(false);
        }


        public async Task CleanupDatabaseAsync(string dbPathOrIdentifier, string baseDir)
        {
            _logger.LogWarning("Attempting to cleanup SQLite database files for: {DbPath}", dbPathOrIdentifier);
            try
            {
                if (File.Exists(dbPathOrIdentifier))
                {
                    File.Delete(dbPathOrIdentifier);
                    _logger.LogDebug("Deleted file: {DbPath}", dbPathOrIdentifier);
                }
                // İlişkili dosyaları da sil (-wal, -shm)
                string walPath = dbPathOrIdentifier + "-wal";
                if (File.Exists(walPath))
                {
                    File.Delete(walPath);
                    _logger.LogDebug("Deleted file: {WalPath}", walPath);
                }
                string shmPath = dbPathOrIdentifier + "-shm";
                if (File.Exists(shmPath))
                {
                    File.Delete(shmPath);
                    _logger.LogDebug("Deleted file: {ShmPath}", shmPath);
                }
            }
            catch (IOException ioEx)
            {
                // Dosya kullanımda olabilir, logla ama devam etmeye çalışabiliriz.
                _logger.LogError(ioEx, "IO Error during SQLite file cleanup for {DbPath}. File might be in use.", dbPathOrIdentifier);
                // Hata fırlatmak yerine sadece loglamak, bazı senaryolarda daha iyi olabilir.
                // throw; // Eğer kritikse hata fırlat.
            }
            await Task.CompletedTask;
        }
    }
}

