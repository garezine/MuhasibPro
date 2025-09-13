using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Data.Abstracts.Common
{
    public interface ISistemLogRepository : IGenericRepository<SistemLog>
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
