using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Muhasebe.Data.Database.Helpers;
using Muhasebe.Data.Database.Interfaces.Operations;

namespace Muhasebe.Data.Database.Concreate.Providers.SqlServer
{
    // --- SQL Server Geri Yükleme ---
    public class SqlServerRestoreOperations : IDatabaseRestoreOperations
    {
        private readonly ILogger<SqlServerRestoreOperations> _logger;

        public SqlServerRestoreOperations(ILogger<SqlServerRestoreOperations> logger)
        {
            _logger = logger;
        }

        // Bu metod master'a bağlanmayı gerektirir. connectionString'in master için olduğu varsayılır.
        // VEYA master bağlantısı IConfiguration'dan alınır ya da dinamik oluşturulur.
        // Şimdilik dinamik oluşturma kullanalım.
        public async Task<RestoreResult> RestoreDatabaseAsync(string backupFilePath, string targetDatabaseName, string targetDbDirectory, string targetDbPath) // targetDbPath SQL Server için genelde kullanılmaz ama arayüzde var
        {
            _logger.LogInformation("Starting SQL Server restore for database '{TargetDbName}' from file '{BackupFilePath}'...", targetDatabaseName, backupFilePath);

            // Varsayılan bir connection string'den master'a bağlanmak için yeni string oluşturma
            // Bu, uygulamanın yapılandırmasına bağlıdır. Belki IConfiguration'dan almak daha iyidir.
            // Geçici olarak, master'a bağlanmak için gereken bilgiyi elde ettiğimizi varsayalım.
            // Örnek: DefaultConnection'ı alıp Initial Catalog'ı 'master' yapalım.
            // BU KISIM UYGULAMANIZA GÖRE AYARLANMALIDIR!
            string masterConnectionString = GetMasterConnectionStringFromSomewhere(); // Bu yardımcı metot implemente edilmeli

            if (string.IsNullOrEmpty(masterConnectionString))
            {
                return new RestoreResult(false, "Master veritabanı bağlantı cümlesi alınamadı.");
            }


            try
            {
                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync().ConfigureAwait(false);

                // 1. Aktif bağlantıları kes
                string alterSql = $"IF DB_ID('{targetDatabaseName}') IS NOT NULL ALTER DATABASE [{targetDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                using (var alterCmd = new SqlCommand(alterSql, connection))
                {
                    _logger.LogDebug("Executing: {Sql}", alterSql);
                    await alterCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Set existing database '{TargetDbName}' (if any) to SINGLE_USER mode.", targetDatabaseName);
                }


                // 2. Yedek dosyasındaki mantıksal dosya adlarını al
                string fileListSql = $"RESTORE FILELISTONLY FROM DISK = @backupFilePath;";
                string logicalDataName = null;
                string logicalLogName = null;
                using (var fileListCmd = new SqlCommand(fileListSql, connection))
                {
                    fileListCmd.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                    _logger.LogDebug("Executing: {Sql}", fileListSql);
                    using (var reader = await fileListCmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            string fileType = reader["Type"].ToString().ToUpper();
                            if (fileType == "D") // Data
                            {
                                logicalDataName = reader["LogicalName"].ToString();
                            }
                            else if (fileType == "L") // Log
                            {
                                logicalLogName = reader["LogicalName"].ToString();
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(logicalDataName) || string.IsNullOrEmpty(logicalLogName))
                {
                    throw new InvalidOperationException("Yedek dosyasından mantıksal dosya adları okunamadı.");
                }
                _logger.LogInformation("Logical file names from backup: Data='{LogicalDataName}', Log='{LogicalLogName}'", logicalDataName, logicalLogName);


                // 3. Fiziksel dosya yollarını oluştur
                string physicalDataPath = Path.Combine(targetDbDirectory, $"{targetDatabaseName}.mdf");
                string physicalLogPath = Path.Combine(targetDbDirectory, $"{targetDatabaseName}_log.ldf");
                _logger.LogDebug("Target physical paths: Data='{PhysicalDataPath}', Log='{PhysicalLogPath}'", physicalDataPath, physicalLogPath);


                // 4. Restore komutunu oluştur ve çalıştır
                string restoreSql = $@"
                    RESTORE DATABASE [{targetDatabaseName}]
                    FROM DISK = @backupFilePath
                    WITH REPLACE, RECOVERY,
                    MOVE @logicalDataName TO @physicalDataPath,
                    MOVE @logicalLogName TO @physicalLogPath;";

                using (var restoreCmd = new SqlCommand(restoreSql, connection))
                {
                    restoreCmd.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                    restoreCmd.Parameters.AddWithValue("@logicalDataName", logicalDataName);
                    restoreCmd.Parameters.AddWithValue("@physicalDataPath", physicalDataPath);
                    restoreCmd.Parameters.AddWithValue("@logicalLogName", logicalLogName);
                    restoreCmd.Parameters.AddWithValue("@physicalLogPath", physicalLogPath);
                    restoreCmd.CommandTimeout = 7200; // Restore uzun sürebilir

                    _logger.LogDebug("Executing RESTORE DATABASE command...");
                    await restoreCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                _logger.LogInformation("SQL Server restore completed successfully for database '{TargetDbName}'.", targetDatabaseName);
                return new RestoreResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL Server restore failed for database '{TargetDbName}'.", targetDatabaseName);
                // Başarısız restore sonrası kısmen oluşturulmuş dosyaları temizlemek gerekebilir.
                // Veya veritabanını tekrar DROP etmek.
                return new RestoreResult(false, $"SQL Server geri yükleme hatası: {ex.Message}");
            }
        }

        // BU YARDIMCI METOT UYGULAMANIZIN YAPILANDIRMASINA GÖRE DOLDURULMALIDIR!
        private string GetMasterConnectionStringFromSomewhere()
        {
            // Örnek: Sabit kodlanmış (önerilmez) veya IConfiguration'dan okuma
            // return "Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            // VEYA IConfiguration inject edip okuyun:
            // return _configuration.GetConnectionString("MasterConnection");
            _logger.LogWarning("GetMasterConnectionStringFromSomewhere() needs to be implemented based on application configuration!");
            // Geçici olarak null dönelim ki hata versin.
            return null;
            // Veya varsayılan bağlantıdan türetme:
            /* try {
                 var builder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection")); // DefaultConnection olduğunu varsayalım
                 builder.InitialCatalog = "master";
                 return builder.ConnectionString;
             } catch (Exception ex) {
                 _logger.LogError(ex, "Failed to build master connection string from default.");
                 return null;
             }*/
        }
        private string GetMasterConnectionString(string connectionString) // Tekrar eklendi (Deletion'daki gibi)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master"
                };
                // Güvenlik bilgilerini koru
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build master connection string from provided string.");
                return null;
            }
        }

    }
}
