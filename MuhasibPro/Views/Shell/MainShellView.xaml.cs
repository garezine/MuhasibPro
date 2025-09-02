using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Muhasebe.Business.Services.Abstract.Common;
using MuhasibPro.Helpers;
using MuhasibPro.Services.Common;
using MuhasibPro.Infrastructure.Models;
using MuhasibPro.Infrastructure.Services;
using MuhasibPro.Infrastructure.Services.Abstract.Common;
using MuhasibPro.ViewModels.ViewModel.Settings;
using MuhasibPro.ViewModels.ViewModel.Shell;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainShellView : Page
    {
        private INavigationService _navigationService = null;
        private IThemeSelectorService themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
        public MainShellView()
        {
            ViewModel = Ioc.Default.GetService<MainShellViewModel>();
            this.InitializeComponent();
            InitializeContext();
            InitializeNavigation();
            App.MainWindow.SetTitleBar(AppTitleBar);
            App.MainWindow.Activated += MainWindow_Activated;
            AppTitleBarText.Text = "MuhasibPro";

        }
        public void InitializeContext()
        {
            var context = Ioc.Default.GetService<IContextService>();
            context.InitializeWithViewType(DispatcherQueue, ViewType.MainShell, this, "MainShellView");

        }

        public MainShellViewModel ViewModel { get; }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            InitializeContext();
            App.AppTitlebar = AppTitleBarText as UIElement;
            if (navigationView != null && frame != null)
            {
                InitializeNavigation();
                navigationView.SelectionChanged -= OnSelectionChanged; // Duplicate event önlemek için
                navigationView.SelectionChanged += OnSelectionChanged;
            }
        }

        public void InitializeNavigation()
        {
            _navigationService = Ioc.Default.GetService<INavigationService>();
            if (_navigationService != null && frame != null)
            {
                _navigationService.Initialize(frame);
                frame.Navigated -= OnFrameNavigated; // Duplicate event önlemek için
                frame.Navigated += OnFrameNavigated;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.LoadAsync(e.Parameter as ShellArgs);
            ViewModel.Subscribe();
            foreach (var item in ViewModel.NavigationItems)
            {
                navigationView.MenuItems.Add(item.Children);
            }
        }


        private void NavigationViewControl_DisplayModeChanged(
            NavigationView sender,
            NavigationViewDisplayModeChangedEventArgs args)
        {
            AppTitleBar.Margin = new Thickness()
            {
                Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
                Top = AppTitleBar.Margin.Top,
                Right = AppTitleBar.Margin.Right,
                Bottom = AppTitleBar.Margin.Bottom
            };
        }

        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            InitializeContext();
            if (args.SelectedItem is NavigationItem item)
            {
                ViewModel.NavigateTo(item.ViewModel);
            }
            else if (args.IsSettingsSelected)
            {
                ViewModel.NavigateTo(typeof(SettingsViewModel));
            }
            UpdateBackButton();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Unload();
            ViewModel.Unsubscribe();
        }

        private void OnNavigationViewBackButton(object sender, RoutedEventArgs e)
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            var targetType = NavigationService.GetViewModel(e.SourcePageType);
            switch (targetType.Name)
            {
                case "SettingsViewModel":
                    ViewModel.SelectedItem = navigationView.SettingsItem;
                    break;
                default:
                    ViewModel.SelectedItem = ViewModel.NavigationItems
                        .Where(r => r.ViewModel == targetType)
                        .FirstOrDefault();
                    break;
            }
            if (frame != null && frame.Content?.GetType() != targetType)
            {
                frame.Navigate(targetType);
            }
            UpdateBackButton();
        }

        private void UpdateBackButton() { NavigationViewBackButton.IsEnabled = _navigationService.CanGoBack; }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            TitleBarHelper.UpdateTitleBar(RequestedTheme);
            KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
            KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(
            VirtualKey key,
            VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(
            KeyboardAccelerator sender,
            KeyboardAcceleratorInvokedEventArgs args)
        {
            var navigationService = Ioc.Default.GetService<INavigationService>();

            var result = navigationService.CanGoBack;

            args.Handled = result;
        }

        public ICommand LogoutCommand => new RelayCommand(Logout);

        private async void Logout()
        {
            var dialogService = ViewModel.DialogService;
            ContentDialog dialog = new ContentDialog();
            dialog.Title = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                    {
                        new SymbolIcon(Symbol.Important),
                        new TextBlock { Text = "Uyarı", Margin = new Thickness(20, 0, 0, 0) },
                    }
            };
            dialog.Content = "Çıkmak istedinize emin misiniz!";
            dialog.PrimaryButtonText = "Oturumu Kapat";
            dialog.SecondaryButtonText = "Uygulamadan Çık";
            dialog.CloseButtonText = "İptal";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.RequestedTheme = themeSelectorService.Theme;
            var logout = await dialogService.ShowCustomDialogAsync(dialog);
            if (logout == ContentDialogResult.Primary)
            {
                var authentication = Ioc.Default.GetService<IAuthenticationService>();
                authentication.Logout();
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
            else if (logout == ContentDialogResult.Secondary)
            {
                ViewModel.Unload();
                Application.Current.Exit();
            }
        }

        public bool IsSistemDatabaseConnection => false;
    }
}

