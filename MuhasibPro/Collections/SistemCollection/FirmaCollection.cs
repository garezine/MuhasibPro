using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Contracts.SistemServices;
using MuhasibPro.Infrastructure.VirtualCollection;

namespace MuhasibPro.Collections.SistemCollection
{
    public class FirmaCollection : VirtualCollection<FirmaModel>
    {
        private DataRequest<Firma> _dataRequest = null;
        bool MustExploreDeepExceptions { get; set; }
        public FirmaCollection(IFirmaService firmaService, ILogService logService, bool mustExploreDeepExceptions = false) : base(logService)
        { FirmaService = firmaService; }

        public IFirmaService FirmaService { get; }

        private FirmaModel _defaultItem = FirmaModel.CreateEmpty();

        public override FirmaModel DefaultItem => _defaultItem;

        public async Task LoadAsync(DataRequest<Firma> dataRequest)
        {
            try
            {
                _dataRequest = dataRequest;

                Count = await FirmaService.GetFirmalarCountAsync(_dataRequest);
                var range = await FirmaService.GetFirmalarAsync(0, RangeSize, _dataRequest);
                Ranges[0] = range.Data;
            }
            catch (Exception)
            {
                Count = 0;
                throw;
            }
        }

        public override async Task<IList<FirmaModel>> FetchDataAsync(int pageIndex, int pageSize)
        {
            try
            {
                var fetch = await FirmaService.GetFirmalarAsync(pageIndex * pageSize, pageSize, _dataRequest);
                return fetch.Data;
            }
            catch (Exception ex)
            {
                LogException("Firma Koleksiyon", "Getir", ex);
            }
            return null;
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
