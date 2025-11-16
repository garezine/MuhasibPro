using Microsoft.Extensions.Logging;
using Muhasib.Data.DataContext;
using Muhasib.Data.DataContext.Factories;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class TenantDbContextAccessor : ITenantDbContextAccessor
    {
        private readonly ITenantSelectionManager _tenantSelectionManager;
        private readonly IAppDbContextFactory _contextFactory;
        private readonly ILogger<TenantDbContextAccessor> _logger;

        public TenantDbContextAccessor(
            ITenantSelectionManager tenantSelectionManager,
            IAppDbContextFactory contextFactory,
            ILogger<TenantDbContextAccessor> logger)
        {
            _tenantSelectionManager = tenantSelectionManager;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public AppDbContext GetCurrentTenantDbContextAsync()
        {
            var currentTenant = _tenantSelectionManager.GetCurrentTenant();

            if (currentTenant == null || !currentTenant.IsLoaded)
            {
                _logger.LogWarning("Aktif tenant seçilmemiş");
                throw new InvalidOperationException("Aktif tenant seçilmemiş. Lütfen önce bir mali dönem seçin.");
            }

            _logger.LogDebug(
                "Creating DbContext for tenant: {MaliDonemId}, Database: {DatabaseName}",
                currentTenant.MaliDonemId,
                currentTenant.DatabaseName);
            var factory = _contextFactory.CreateForTenantAsync(currentTenant.MaliDonemId);
            return factory.GetAwaiter().GetResult();
        }

        public bool HasActiveTenant()
        {
            var currentTenant = _tenantSelectionManager.GetCurrentTenant();
            return currentTenant?.IsLoaded ?? false;
        }
    }
}
