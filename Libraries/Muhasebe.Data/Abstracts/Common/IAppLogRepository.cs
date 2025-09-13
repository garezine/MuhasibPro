using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Data.Abstracts.Common
{
    public interface IAppLogRepository : IGenericRepository<AppLog>
    {
        Task<AppLog> GetLogAsync(long id);
        Task<IList<AppLog>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request);
        Task<IList<AppLog>> GetLogKeysAsync(int skip, int take, DataRequest<AppLog> request);
        Task<int> GetLogsCountAsync(DataRequest<AppLog> request);
        Task<int> CreateLogAsync(AppLog appLog);
        Task<int> DeleteLogsAsync(params AppLog[] logs);
        Task MarkAllAsReadAsync();
    }
}
