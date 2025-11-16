using MuhasibPro.Helpers;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.ViewWindow
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListWindow : Window
    {
        public ListWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;


            //OverlappedPresenter presenter = OverlappedPresenter.CreateForDialog();
            //SetWindowOwner(owner: WindowHelper.MainWindow);
            //presenter.IsModal = true;
            //presenter.IsMaximizable = true;
            //AppWindow.SetPresenter(presenter);
            //AppWindow.Show();
            Closed += ListWindow_Closed;
        }

        private void ListWindow_Closed(object sender, WindowEventArgs args)
        {
            CustomWindowHelper.MainWindow.Activate();
        }
        // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    }
}
