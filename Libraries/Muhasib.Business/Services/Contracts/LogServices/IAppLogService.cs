using Muhasib.Business.Models.MuhasebeModel;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Contracts.LogServices
{
    public interface IAppLogService
    {
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task<AppLogModel> GetAppLogAsync(long id);
        Task<IList<AppLogModel>> GetAppLogsAsync(int skip, int take, DataRequest<AppLog> request);
        Task<int> GetAppLogsCountAsync(DataRequest<AppLog> request);
        Task<int> DeleteAppLogAsync(AppLogModel model);
        Task<int> DeleteAppLogRangeAsync(int index, int length, DataRequest<AppLog> request);
        Task AppLogMarkAllAsReadAsync();

    }
}
