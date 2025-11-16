using Muhasib.Business.Models.SistemModel;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.MaliDonem
{
    public class CalismaDonemDetailsArgs
    {
        public long FirmaID { get; set; }
        public long CalismaDonemID { get; set; }
        public bool IsNew => CalismaDonemID <= 0;

    }
    public class MaliDonemDetailsViewModel : GenericDetailsViewModel<MaliDonemModel>
    {
        public MaliDonemDetailsViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }

        public override bool ItemIsNew => throw new NotImplementedException();

        protected override Task<bool> ConfirmDeleteAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteItemAsync(MaliDonemModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> SaveItemAsync(MaliDonemModel model)
        {
            throw new NotImplementedException();
        }
    }
}
