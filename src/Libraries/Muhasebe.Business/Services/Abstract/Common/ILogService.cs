using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Enum;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface ILogService
    {
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task LogInformationAsync(string source, string action, string message, string description = "");
        Task LogErrorAsync(string source, string action, string message, string description = "");
        Task LogExceptionAsync(string source, string action, Exception ex);
        Task<AppLogModel> GetLogAsync(long id);
        Task<IList<AppLogModel>> GetLogsAsync();
        Task<IList<AppLogModel>> GetLogsAsync(int skip, int take,DataRequest<AppLog> request);
        Task<int> GetLogsCountAsync(DataRequest<AppLog> request);
        Task<int> DeleteLogAsync(long id);
        Task<int> DeleteLogRangeAsync(int index, int length,DataRequest<AppLog> request);
        Task MarkAllAsReadAsync();
    }
}
