using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase
{
    public interface ISistemDatabaseService
    {
        Task<ApiDataResponse<bool>> InitializeDatabaseAsync();
        //connection methods
        Task<ApiDataResponse<bool>> ValidateConnectionAsync();
        Task<ApiDataResponse<string>> TestConnectionAsync();
        
    }
}
