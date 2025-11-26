// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;

namespace MuhasibPro.Views.Loggings.List
{
    public sealed partial class SistemLogsList : UserControl
    {
        public SistemLogsList() { InitializeComponent(); }
        #region ViewModel
        public SistemLogListViewModel ViewModel
        {
            get { return (SistemLogListViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(SistemLogListViewModel),
            typeof(SistemLogsList),
            new PropertyMetadata(null));

        #endregion
        private void Page_Onloaded(object sender, RoutedEventArgs e)
        {
            if (pageTableView != null && dataList.ConfigControl != null)
            {
                dataList.ConfigControl.AttachTableView(pageTableView);
            }
        }
    }
}
