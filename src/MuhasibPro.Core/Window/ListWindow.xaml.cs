using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MuhasibPro.Core.Infrastructure.Helpers;
using System.Runtime.InteropServices;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Core.GWindow
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
            WindowHelper.MainWindow.Activate();
        }

        private void SetWindowOwner(Window owner)
        {
            // Get the HWND (window handle) of the owner window (main window).
            IntPtr ownerHwnd = WindowNative.GetWindowHandle(owner);

            // Get the HWND of the AppWindow (modal window).
            IntPtr ownedHwnd = Win32Interop.GetWindowFromWindowId(AppWindow.Id);

            // Set the owner window using SetWindowLongPtr for 64-bit systems
            // or SetWindowLong for 32-bit systems.
            if (IntPtr.Size == 8) // Check if the system is 64-bit
            {
                SetWindowLongPtr(ownedHwnd, -8, ownerHwnd); // -8 = GWLP_HWNDPARENT
            }
            else // 32-bit system
            {
                SetWindowLong(ownedHwnd, -8, ownerHwnd); // -8 = GWL_HWNDPARENT
            }
        }
        // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    }
}

