using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Login
{
    public sealed partial class UserProfileControl : UserControl
    {

        public UserProfileControl()
        {
            this.InitializeComponent();
            InitializeEvents();
        }
        private void InitializeEvents()
        {
            CompactView.Tapped += OnCompactViewTapped;
            CompactView.PointerEntered += OnCompactViewPointerEntered;
            CompactView.PointerExited += OnCompactViewPointerExited;
        }

        private void OnCompactViewTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(MainGrid);
        }

        private void OnCompactViewPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HoverAnimation.Begin();
        }

        private void OnCompactViewPointerExited(object sender, PointerRoutedEventArgs e)
        {
            NormalAnimation.Begin();
        }
        #region DisplayName
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region DisplayRol
        public string DisplayRol
        {
            get { return (string)GetValue(DisplayRolProperty); }
            set { SetValue(DisplayRolProperty, value); }
        }

        public static readonly DependencyProperty DisplayRolProperty = DependencyProperty.Register(nameof(DisplayRol), typeof(string), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region UserName
        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(nameof(UserName), typeof(string), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region Rol
        public string Rol
        {
            get { return (string)GetValue(RolProperty); }
            set { SetValue(RolProperty, value); }
        }

        public static readonly DependencyProperty RolProperty = DependencyProperty.Register(nameof(Rol), typeof(string), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region Eposta
        public string Eposta
        {
            get { return (string)GetValue(EpostaProperty); }
            set { SetValue(EpostaProperty, value); }
        }

        public static readonly DependencyProperty EpostaProperty = DependencyProperty.Register(nameof(Eposta), typeof(string), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region LogoutCommand
        public ICommand LogoutCommand
        {
            get { return (ICommand)GetValue(LogoutCommandProperty); }
            set { SetValue(LogoutCommandProperty, value); }
        }

        public static readonly DependencyProperty LogoutCommandProperty = DependencyProperty.Register(nameof(LogoutCommand), typeof(ICommand), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion

        #region UserProfileOpenCommand
        public ICommand UserProfileOpenCommand
        {
            get { return (ICommand)GetValue(UserProfileOpenCommandProperty); }
            set { SetValue(UserProfileOpenCommandProperty, value); }
        }

        public static readonly DependencyProperty UserProfileOpenCommandProperty = DependencyProperty.Register(nameof(UserProfileControl), typeof(ICommand), typeof(NamePasswordControl), new PropertyMetadata(null));
        #endregion
    }
}
