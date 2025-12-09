using Muhasib.Business.Services.Contracts.DatabaseServices.SistemDatabase;
using Muhasib.Data.Utilities.Responses;

namespace Muhasib.Business.Services.Concrete.DatabaseServices.SistemDatabase
{
    public interface ISistemDatabaseService
    {
        Task<ApiDataResponse<bool>> InitializeDatabaseAsync();
        //connection methods
        Task<ApiDataResponse<bool>> ValidateConnectionAsync();
        ISistemDatabaseOperationService SistemDatabaseOperation { get; }

    }
}
