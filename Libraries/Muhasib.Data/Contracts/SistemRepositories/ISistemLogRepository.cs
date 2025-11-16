using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Contracts.SistemRepositories
{
    public interface ISistemLogRepository : IRepository<SistemLog>
    {
        Task<SistemLog> GetLogAsync(long id);
        Task<IList<SistemLog>> GetLogsAsync(int skip, int take, DataRequest<SistemLog> request);
        Task<IList<SistemLog>> GetLogKeysAsync(int skip, int take, DataRequest<SistemLog> request);
        Task<int> GetLogsCountAsync(DataRequest<SistemLog> request);
        Task<int> CreateLogAsync(SistemLog appLog);
        Task<int> DeleteLogsAsync(params SistemLog[] logs);
        Task MarkAllAsReadAsync();
    }
}
