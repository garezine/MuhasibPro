using Muhasebe.Business.Models.DbModel.AppModel.Sistem;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface ILookupTables
    {
        Task InitializeAsync();
        IList<IllerModel> IllerList { get; }

        string GetIller(long id);
    }

    public class LookupTablesProxy
    {
        public static ILookupTables Instance { get; set; }
    }
}
