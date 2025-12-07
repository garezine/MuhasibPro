using Muhasib.Business.Models.SistemModel;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Firma
{
    public sealed partial class FirmaCard : UserControl
    {
        public FirmaCard()
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

        #region Item
        public FirmaModel Item
        {
            get { return (FirmaModel)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(FirmaModel), typeof(FirmaCard), new PropertyMetadata(null));
        #endregion
    }
}
