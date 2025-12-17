// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;

namespace MuhasibPro.Views.Firma
{
    public sealed partial class FirmaDetails : UserControl
    {
        public FirmaDetails()
        {
            InitializeComponent();            
        }
        #region ViewModel
        public FirmaDetailsWithMaliDonemlerViewModel ViewModel
        {
            get { return (FirmaDetailsWithMaliDonemlerViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(FirmaDetailsWithMaliDonemlerViewModel), typeof(FirmaDetails), new PropertyMetadata(null));
        #endregion
        public void SetFocus()
        {
            details.SetFocus();
        }
        public int GetRowSpan(bool isItemNew)
        {
            return isItemNew ? 2 : 1;
        }
      
    }
}
