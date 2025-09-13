using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Business.Services.Abstracts.Common
{
    public interface IAppLogService
    {
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task<AppLogModel> GetLogAsync(long id);
        Task<IList<AppLogModel>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request);
        Task<int> GetLogsCountAsync(DataRequest<AppLog> request);
        Task<int> CreateLogAsync(AppLog appLog);
        Task<int> DeleteLogAsync(AppLogModel model);
        Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<AppLog> request);
        Task MarkAllAsReadAsync();
    }
}
