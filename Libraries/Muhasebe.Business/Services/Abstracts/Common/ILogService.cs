using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Business.Services.Abstracts.Common
{
    public interface ILogService
    {
        ISistemLogService SistemLogService { get; }
        IAppLogService AppLogService { get; }
        public Task<IList<SistemLogModel>> GetSistemLogsAsync(DataRequest<SistemLog> request);
        public Task<IList<AppLogModel>> GetAppLogsAsync(DataRequest<AppLog> request);
    }
}
