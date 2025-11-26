using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;
using Muhasib.Data.Managers.DatabaseManager.Models;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    /// <summary>
    /// Tenant (MaliDonem) context yönetimi sağlar.
    /// Tenant switching, current state management ve validation işlemlerini handle eder.
    /// Business katmanında yer alır ve tenant lifecycle'ını yönetir.
    /// </summary>
    public class TenantSelectionManager : ITenantSelectionManager
    {
        private readonly ITenantConnectionManager _connectionManager;
        private readonly IMaliDonemRepository _maliDonemRepo;
        private TenantContext _currentTenant;

        public bool IsTenantLoaded => _currentTenant?.IsLoaded ?? false;

        public TenantSelectionManager(
            ITenantConnectionManager connectionManager,
            IMaliDonemRepository maliDonemRepo)
        {
            _connectionManager = connectionManager;
            _maliDonemRepo = maliDonemRepo;
            _currentTenant = TenantContext.Empty;
        }

        public async Task<TenantContext> SwitchToTenantAsync(long maliDonemId)
        {
            // 1. Connection test - DÜZELTİLDİ
            var testResult = await _connectionManager.TestConnectionAsync(maliDonemId);
            if (testResult != ConnectionTestResult.Success)
                throw new Exception($"Tenant connection failed: {testResult}");

            // 2. Database bilgilerini al
            var maliDonemDb = await _maliDonemRepo.GetByMaliDonemIdAsync(maliDonemId);
            var connectionString = await _connectionManager.GetConnectionStringAsync(maliDonemId);

            // 3. Context oluştur - DÜZELTİLDİ
            _currentTenant = new TenantContext
            {
                MaliDonemId = maliDonemId,
                DatabaseName = maliDonemDb.DBName,
                ConnectionString = connectionString,
                LoadedAt = DateTime.Now
            };

            TenantChanged?.Invoke(_currentTenant);
            return _currentTenant;
        }

        public TenantContext GetCurrentTenant() => _currentTenant;

        public async Task<bool> ValidateTenantAsync(long maliDonemId)
        {
            var result = await _connectionManager.ValidateTenantAsync(maliDonemId);
            return result.IsValid;
        }
        public event Action<TenantContext> TenantChanged;
    }
}
