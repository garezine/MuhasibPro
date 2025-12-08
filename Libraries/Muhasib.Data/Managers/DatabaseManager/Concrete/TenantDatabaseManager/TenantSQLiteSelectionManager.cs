using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantSqliteManager;
using Muhasib.Domain.Enum;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantSqliteManager
{
    public class TenantSQLiteSelectionManager : ITenantSQLiteSelectionManager
    {
        private readonly IAppDbContextFactory _dbContextFactory;
        private readonly ITenantSQLiteConnectionStringFactory _connectionStringFactory;
        private TenantContext _currentTenant;
        private readonly ILogger<TenantSQLiteSelectionManager> _logger;
        private readonly object _tenantLock = new object();

        // ⭐ Constructor düzeltildi: Kullanılmayan dependency çıkarıldı
        public TenantSQLiteSelectionManager(
            IAppDbContextFactory dbContextFactory,
            ITenantSQLiteConnectionStringFactory connectionStringFactory,
            ILogger<TenantSQLiteSelectionManager> logger) // ⭐ connectionManager ÇIKARILDI
        {
            _currentTenant = TenantContext.Empty;
            _dbContextFactory = dbContextFactory;
            _connectionStringFactory = connectionStringFactory;
            _logger = logger;
        }

        public bool IsTenantLoaded => _currentTenant?.IsLoaded ?? false;

        public event Action<TenantContext> TenantChanged;

        public TenantContext GetCurrentTenant() => _currentTenant;

        public async Task<TenantContext> SwitchToTenantAsync(string databaseName)
        {
            lock (_tenantLock)
            {
                if (_currentTenant.DatabaseName == databaseName && _currentTenant.IsLoaded)
                {
                    _logger.LogDebug("Zaten aktif tenant: {DatabaseName}", databaseName);
                    return _currentTenant;
                }
            }

            // ⭐ Connection testi (bool döner, exception atmaz)
            bool testResult = await _connectionStringFactory.TestConnectionStringAsync(databaseName);
            if (!testResult)
            {
                _logger.LogError("Tenant bağlantısı kurulamadı: {DatabaseName}", databaseName);
                throw new InvalidOperationException($"Tenant bağlantısı kurulamadı: {databaseName}");
            }

            var connectionString = _connectionStringFactory.CreateConnectionString(databaseName);
            var newTenant = new TenantContext
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName,
                DatabaseType = DatabaseType.SQLite,
                LoadedAt = DateTime.Now
            };

            lock (_tenantLock)
            {
                _currentTenant = newTenant;
            }

            TenantChanged?.Invoke(newTenant);
            _logger.LogInformation("Tenant değiştirildi: {DatabaseName}", databaseName);
            return _currentTenant;
        }

        public AppDbContext CreateTenantDbContext()
        {
            var currentTenant = GetCurrentTenant();

            if (!currentTenant.IsLoaded)
            {
                _logger.LogWarning("Aktif tenant seçilmemiş");
                throw new InvalidOperationException(
                    "Aktif tenant seçilmemiş. Önce SwitchToTenantAsync() ile tenant seçin.");
            }

            _logger.LogDebug("Creating DbContext for: {DatabaseName}", currentTenant.DatabaseName);
            return _dbContextFactory.CreateContext(currentTenant.DatabaseName);
        }

        // ⭐ Metod ismi ve imzası düzeltildi
        public Task<string> GetCurrentTenantConnectionStringAsync()
        {
            try
            {
                var current = GetCurrentTenant();
                return Task.FromResult(current.IsLoaded ? current.ConnectionString : string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Current tenant connection string alınamadı");
                return Task.FromResult(string.Empty);
            }
        }

        public void ClearCurrentTenant()
        {
            lock (_tenantLock)
            {
                _currentTenant = TenantContext.Empty;
            }

            _logger.LogInformation("Tenant bağlantısı temizlendi");
            TenantChanged?.Invoke(TenantContext.Empty);
        }
    }
}