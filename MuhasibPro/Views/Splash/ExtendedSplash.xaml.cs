// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Splash
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page
    {
        public static Queue<string> StatusMessages { get; } = new Queue<string>();
        public ExtendedSplash()
        {
            InitializeComponent();
        }
    }
}
