
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.ViewModels.ViewModels.Firmalar;

namespace MuhasibPro.Views.Firmalar.List
{
    public sealed partial class FirmalarList : UserControl
    {
        public FirmalarList()
        {
            InitializeComponent();
        }
        #region ViewModel
        public FirmaListViewModel ViewModel
        {
            get { return (FirmaListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(FirmaListViewModel), typeof(FirmalarList), new PropertyMetadata(null));
        #endregion
    }
}
