using Muhasib.Business.Models.SistemModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.MaliDonem
{
    public sealed partial class MaliDonemCard : UserControl
    {
        public MaliDonemCard()
        {
            InitializeComponent();
        }
        #region Item
        public MaliDonemModel Item
        {
            get { return (MaliDonemModel)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(MaliDonemModel), typeof(MaliDonemCard), new PropertyMetadata(null));
        #endregion
    }
}
