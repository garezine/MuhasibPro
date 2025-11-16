using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantDatabaseLifecycleService
    {
        Task<ApiDataResponse<string>> CreateDatabaseAsync(string databaseName);
        Task<ApiDataResponse<bool>> DeleteDatabaseAsync(string databaseName);
        Task<ApiDataResponse<bool>> DatabaseExistsAsync(string databaseName);
        ApiDataResponse<string> GenerateDatabaseNameAsync(string firmaKodu, int maliYil);
    }
}
