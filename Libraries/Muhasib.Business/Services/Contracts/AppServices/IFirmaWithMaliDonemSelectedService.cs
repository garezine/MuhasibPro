using Muhasib.Business.Models.SistemModel;
using Muhasib.Data.DataContext;

namespace Muhasib.Business.Services.Contracts.AppServices
{
    public interface IFirmaWithMaliDonemSelectedService
    {
        FirmaModel SelectedFirma { get; set; }
        MaliDonemModel SelectedMaliDonem { get; set; }
        TenantContext ConnectedTenantDb { get; set; }
        event Action StateChanged;
    }
}
