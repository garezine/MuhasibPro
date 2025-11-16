// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Muhasib.Business.Models.SistemModel;
using MuhasibPro.ViewModels.ViewModels.MaliDonem;

namespace MuhasibPro.Views.Firmalar.Details
{
    public sealed partial class FirmaCalismaDonemler : UserControl
    {
        public FirmaCalismaDonemler()
        {
            InitializeComponent();
        }
        #region ViewModel
        public MaliDonemListViewModel ViewModel
        {
            get { return (MaliDonemListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MaliDonemListViewModel), typeof(MaliDonemModel), new PropertyMetadata(null));
        #endregion
    }
}
