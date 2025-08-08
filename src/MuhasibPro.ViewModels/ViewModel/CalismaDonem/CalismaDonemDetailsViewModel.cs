using Muhasebe.Business.Models.DbModel.AppModel;
using MuhasibPro.Core.Services.Common;
using MuhasibPro.Infrastructure.ViewModels.Common;

namespace MuhasibPro.ViewModels.ViewModel.CalismaDonem
{
    public class CalismaDonemDetailsArgs
    {
        public long FirmaID { get; set; }
        public long CalismaDonemID { get; set; }
        public bool IsNew => CalismaDonemID <= 0;

    }
    public class CalismaDonemDetailsViewModel : GenericDetailsViewModel<CalismaDonemModel>
    {
        public CalismaDonemDetailsViewModel(ICommonServices commonServices) : base(commonServices)
        {
        }

        public override bool ItemIsNew => throw new NotImplementedException();

        protected override Task<bool> ConfirmDeleteAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteItemAsync(CalismaDonemModel model)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> SaveItemAsync(CalismaDonemModel model)
        {
            throw new NotImplementedException();
        }
    }
}
