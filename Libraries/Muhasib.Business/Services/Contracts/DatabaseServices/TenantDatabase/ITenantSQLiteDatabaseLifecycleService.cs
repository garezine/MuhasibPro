using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase
{
    public interface ITenantSQLiteDatabaseLifecycleService
    {
        Task<ApiDataResponse<string>> CreateOrUpdateDatabaseAsync(string databaseName);
        Task<ApiDataResponse<bool>> DeleteDatabaseAsync(string databaseName);     
        ApiDataResponse<string> GenerateDatabaseName(string firmaKodu, int maliYil);
        Task<ApiDataResponse<bool>> ValidateConnectionAsync(string databaseName);
    }
}
