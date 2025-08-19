using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Muhasebe.Business.Models.DbModel.AppModel;
using MuhasibPro.ViewModels.ViewModel.CalismaDonem;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Firmalar.Details
{
    public sealed partial class FirmaCalismaDonemler : UserControl
    {
        public FirmaCalismaDonemler()
        {
            InitializeComponent();
        }
        #region ViewModel
        public CalismaDonemListViewModel ViewModel
        {
            get { return (CalismaDonemListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(CalismaDonemListViewModel), typeof(MaliDonemModel), new PropertyMetadata(null));
        #endregion
    }
}
