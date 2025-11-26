// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;

namespace MuhasibPro.Views.Loggings.Details
{
    public sealed partial class SistemLogsDetails : UserControl
    {
        public SistemLogsDetails()
        {
            InitializeComponent();
        }
        #region ViewModel
        public SistemLogDetailsViewModel ViewModel
        {
            get { return (SistemLogDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(SistemLogDetailsViewModel), typeof(SistemLogsDetails), new PropertyMetadata(null));
        #endregion
    }
}
