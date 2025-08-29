using Muhasebe.Business.Models.AppModel;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Interfaces.App;

namespace Muhasebe.Business.Services.Concreate.Common
{
    public class LookupTables : ILookupTables
    {

        public LookupTables(ILogService logService, IDefaultDataListRepository entityTablesList)
        {
            LogService = logService;
            EntityTablesList = entityTablesList;
        }
        public ILogService LogService { get;}
        public IDefaultDataListRepository EntityTablesList { get; }
        public IList<IllerModel> IllerList { get; private set; }

        public string GetIller(long id)
        {
            return IllerList.Where(r=> r.Id == id).Select(r=> r.IlAdi).FirstOrDefault();
        }

        public async Task InitializeAsync()
        {
            IllerList = await GetIllerAsync();
        }
        private async Task<IList<IllerModel>> GetIllerAsync()
        {
            try
            {
                var items = await EntityTablesList.GetIllerAsync();
                return items.Select(r => new IllerModel
                {
                    Id = r.Id,
                    IlAdi = r.IlAdi,
                })
                    .ToList();
            }
            catch (Exception ex)
            {
                LogException("Iller listesi", "Iller listesi alınamadı", ex);
            }
            return new List<IllerModel>();

        }
        private async void LogException(string source, string action, Exception exception)
        {
            await LogService.WriteAsync(Domain.Enum.LogType.Hata, source, action, exception);
        }
    }
}
