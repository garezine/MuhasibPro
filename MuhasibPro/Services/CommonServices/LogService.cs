using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Services.Concrete.Common;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Collections.AppCollection;

namespace MuhasibPro.Services.CommonServices
{
    public class LogService : ILogService
    {
        private readonly ISistemLogRepository _sistemLogRepository;
        private readonly IAppLogRepository _appLogRepository;
        private readonly IAuthenticationService _authenticator;

        public LogService(
            ISistemLogRepository sistemLogRepository,
            IAppLogRepository appLogRepository,
            IAuthenticationService authenticator)
        {
            _sistemLogRepository = sistemLogRepository;
            _appLogRepository = appLogRepository;
            _authenticator = authenticator;
            SistemLogService = new SistemLogService(_sistemLogRepository, _authenticator);
            AppLogService = new AppLogService(_appLogRepository, _authenticator);
        }

        public ISistemLogService SistemLogService { get; }

        public IAppLogService AppLogService { get; }

        public async Task<IList<AppLogModel>> GetAppLogsAsync(DataRequest<AppLog> request)
        {
            var collection = new AppLogCollection(this);
            await collection.LoadAsync(request);
            return collection;
        }

        public async Task<IList<SistemLogModel>> GetSistemLogsAsync(DataRequest<SistemLog> request)
        {
            var collection = new SistemLogCollection(this);
            await collection.LoadAsync(request);
            return collection;
        }
    }
}
