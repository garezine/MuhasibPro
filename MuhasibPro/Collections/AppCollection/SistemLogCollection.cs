using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Infrastructure.VirtualCollection;

namespace MuhasibPro.Collections.AppCollection
{
    public class SistemLogCollection : VirtualCollection<SistemLogModel>
    {
        private DataRequest<SistemLog> _dataRequest = null;
        bool MustExploreDeepExceptions { get; set; }
        public SistemLogCollection(ILogService logService, bool mustExploreDeepExceptions = false) : base(logService)
        {
            MustExploreDeepExceptions = mustExploreDeepExceptions;
        }
        private SistemLogModel _defaultItem = SistemLogModel.CreateEmpty();
        
        public override SistemLogModel DefaultItem => _defaultItem;

        public async Task LoadAsync(DataRequest<SistemLog> dataRequest)
        {
            try
            {
                _dataRequest = dataRequest;
                Count = await LogService.SistemLogService.GetLogsCountAsync(_dataRequest);
                Ranges[0] = await FetchDataAsync(0, RangeSize);
            }
            catch (Exception ex)
            {
                Count = 0;
                throw ex;
            }
        }
        public async override Task<IList<SistemLogModel>> FetchDataAsync(int rangeIndex, int rangeSize)
        {
            try
            {
                return await LogService.SistemLogService.GetLogsAsync(rangeIndex * rangeSize, rangeSize, _dataRequest);
            }
            catch (Exception ex)
            {
                LogException("SistemLogCollection", "Fetch", ex);
            }
            return new List<SistemLogModel>();
        }
        protected async void LogException(string source, string action, Exception exception)
        {
            if (MustExploreDeepExceptions == false)
            {
                await LogService.SistemLogService.WriteAsync(LogType.Hata, source, action, exception.Message, exception.ToString());
            }
            else
            {
                await LogService.SistemLogService.WriteAsync(LogType.Hata, source, action, exception);
            }
        }
    }
}
