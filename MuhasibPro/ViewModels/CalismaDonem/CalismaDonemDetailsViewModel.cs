using Muhasebe.Business.Models.SistemModel;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.ViewModels.Common;

namespace MuhasibPro.ViewModels.CalismaDonem
{
    public class CalismaDonemDetailsArgs
    {
        public long FirmaID { get; set; }
        public long CalismaDonemID { get; set; }
        public bool IsNew => CalismaDonemID <= 0;

    }
    public class CalismaDonemDetailsViewModel : GenericDetailsViewModel<MaliDonemModel>
    {
        public CalismaDonemDetailsViewModel(ICommonServices commonServices) : base(commonServices)
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
