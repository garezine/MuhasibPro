using MuhasibPro.App.Views.Firma;
using MuhasibPro.Core.Infrastructure.Common;
using MuhasibPro.ViewModels.ViewModel.Firmalar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.App.Views.Firmalar.Details
{
    public sealed partial class FirmalarDetails : UserControl
    {
        public FirmalarDetails()
        {
            InitializeComponent();

        }
        #region ViewModel
        public FirmaDetailsViewModel ViewModel
        {
            get { return (FirmaDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(FirmaDetailsViewModel), typeof(FirmaCard), new PropertyMetadata(null));

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ValidationHelper.RegisterAllControls(this);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ValidationHelper.ClearRegisteredControls();

        }
    }
}
