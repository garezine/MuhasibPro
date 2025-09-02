using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Data.Abstract.Common
{
    public interface ILogRepository
    {
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task<AppLog> GetLogAsync(long id);
        Task<IList<AppLog>> GetLogsAsync();
        Task<IList<AppLog>> GetLogsAsync(int skip, int take,DataRequest<AppLog> request);
        Task<int> GetLogsCountAsync(DataRequest<AppLog> request);        
        Task<int> DeleteLogAsync(long id);
        Task<int> DeleteLogRangeAsync(int index, int length,DataRequest<AppLog> request);
        Task<int> CreateLogAsync(AppLog appLog);
        Task MarkAllAsReadAsync();
    }
}
