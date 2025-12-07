// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

namespace MuhasibPro.Views.Firmalar.Details
{
    public sealed partial class FirmaMaliDonemler : UserControl
    {
        public FirmaMaliDonemler()
        {
            InitializeComponent();
        }
        private void OnLoadFirmaMaliDonemler(object sender, RoutedEventArgs e)
        {
            //if (pageTableView != null && dataList.ConfigControl != null)
            //{
            //    dataList.ConfigControl.AttachTableView(pageTableView);
            //}
        }
        #region ViewModel
        public MaliDonemListViewModel ViewModel
        {
            get { return (MaliDonemListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MaliDonemListViewModel), typeof(FirmaMaliDonemler), new PropertyMetadata(null));
        #endregion 
    }
}
