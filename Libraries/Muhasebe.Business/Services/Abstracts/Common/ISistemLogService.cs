using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Business.Services.Abstracts.Common
{
    public interface ISistemLogService
    {
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task<SistemLogModel> GetLogAsync(long id);
        Task<IList<SistemLogModel>> GetLogsAsync(int skip, int take, DataRequest<SistemLog> request);
        Task<int> GetLogsCountAsync(DataRequest<SistemLog> request);
        Task<int> CreateLogAsync(SistemLog appLog);
        Task<int> DeleteLogAsync(SistemLogModel model);
        Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<SistemLog> request);
        Task MarkAllAsReadAsync();
    }
}
