using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Infrastructure.VirtualCollection;

namespace MuhasibPro.Collections.AppCollection
{
    public class AppLogCollection : VirtualCollection<AppLogModel>
    {
        private DataRequest<AppLog> _dataRequest = null;
        bool MustExploreDeepExceptions { get; set; }
        public AppLogCollection(ILogService logService, bool mustExploreDeepExceptions=false) : base(logService)
        {
            MustExploreDeepExceptions = mustExploreDeepExceptions;
        }

        private AppLogModel _defaultItem = AppLogModel.CreateEmpty();
        public override AppLogModel DefaultItem => _defaultItem;
        public async Task LoadAsync(DataRequest<AppLog> dataRequest)
        {
            try
            {
                _dataRequest = dataRequest;
                Count = await LogService.AppLogService.GetLogsCountAsync(_dataRequest);
                Ranges[0] = await FetchDataAsync(0, RangeSize);
            }
            catch (Exception ex)
            {
                Count = 0;
                throw ex;
            }
        }

        public async override Task<IList<AppLogModel>> FetchDataAsync(int rangeIndex, int rangeSize)
        {
            try
            {
                return await LogService.AppLogService.GetLogsAsync(rangeIndex * rangeSize, rangeSize, _dataRequest);
            }
            catch (Exception ex)
            {
                LogException("AppLogCollection", "Fetch", ex);
            }
            return new List<AppLogModel>();
        }
        protected async void LogException(string source, string action, Exception exception)
        {
            if (MustExploreDeepExceptions == false)
            {
                await LogService.AppLogService.WriteAsync(LogType.Hata, source, action, exception.Message, exception.ToString());
            }
            else
            {
                await LogService.AppLogService.WriteAsync(LogType.Hata, source, action, exception);
            }
        }
    }
}
