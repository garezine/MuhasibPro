// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;

namespace MuhasibPro.Views.Firmalar.List
{
    public sealed partial class FirmalarList : UserControl
    {
        public FirmalarList()
        {
            InitializeComponent();
        }
        private void FirmalarList_Loaded(object sender, RoutedEventArgs e)
        {
            if (pageTableView != null && dataList.ConfigControl != null)
            {
                dataList.ConfigControl.AttachTableView(pageTableView);
            }
        }
        #region ViewModel
        public FirmaListViewModel ViewModel
        {
            get { return (FirmaListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(FirmaListViewModel),
            typeof(FirmalarList),
            new PropertyMetadata(null));
        #endregion
    }
}
