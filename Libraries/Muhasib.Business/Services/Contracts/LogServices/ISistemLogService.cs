using Muhasib.Business.Models.SistemModel;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Services.Contracts.LogServices
{
    public interface ISistemLogService
    {
        Task WriteAsync(LogType type, string source, string action, string message, string description);
        Task WriteAsync(LogType type, string source, string action, Exception ex);
        Task<SistemLogModel> GetSistemLogAsync(long id);
        Task<IList<SistemLogModel>> GetSistemLogsAsync(int skip, int take, DataRequest<SistemLog> request);
        Task<int> GetSistemLogsCountAsync(DataRequest<SistemLog> request);
        Task<int> DeleteSistemLogAsync(SistemLogModel model);
        Task<int> DeleteSistemLogRangeAsync(int index, int length, DataRequest<SistemLog> request);
        Task SistemLogMarkAllAsReadAsync();
    }
}
