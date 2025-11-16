using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.Contracts.SistemRepositories
{
    public interface IAppLogRepository : IRepository<AppLog>
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

