using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.MaliDonem
{
    public sealed partial class MaliDonemDetails : UserControl
    {
        public MaliDonemDetails()
        {
            InitializeComponent();
        }
        #region ViewModel
        public MaliDonemDetailsViewModel ViewModel
        {
            get { return (MaliDonemDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MaliDonemDetailsViewModel), typeof(MaliDonemDetails), new PropertyMetadata(null));
        #endregion
        public void SetFocus()
        {
            details.SetFocus();
        }

    }
}
