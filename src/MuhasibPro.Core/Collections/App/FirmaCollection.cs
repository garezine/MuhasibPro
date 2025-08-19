using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemDb;
using MuhasibPro.Core.Infrastructure.Common.VirtualCollection;

namespace MuhasibPro.Core.Collections.App
{
    public class FirmaCollection : VirtualCollection<FirmaModel>
    {
        private DataRequest<Firma> _dataRequest = null;

        public FirmaCollection(IFirmaService firmaService, ILogService logService) : base(logService)
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
            } catch(Exception)
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
            } catch(Exception ex)
            {
                LogException("Firma Koleksiyon", "Getir", ex);
            }
            return null;
        }
    }
}
