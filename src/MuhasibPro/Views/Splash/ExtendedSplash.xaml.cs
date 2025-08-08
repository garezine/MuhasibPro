using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Configuration;
using MuhasibPro.Core.Services;
using MuhasibPro.Services;
using MuhasibPro.ViewModels.ViewModel.Shell;
using MuhasibPro.Views.Login;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Splash
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page
    {
        private readonly Frame rootFrame;

        public ExtendedSplash()
        {
            InitializeComponent();
            rootFrame = new Frame();
            StartupApp();
            StartLoadingSequence();
            App.MainWindow.IsTitleBarVisible = false;
        }
            
        private async void StartLoadingSequence()
        {
            // Her mesaj için
            foreach (var message in StatusMessage.Message)
            {
                await ShowMessage(message);
                await Task.Delay(400); // Her mesaj 0,4 saniye bekler
            }
            // Yükleme tamamlandýðýnda splash screen'i gizle

            LoadDataAsync();
            await HideSplashScreen();
        }
        private async void StartupApp()
        {
            StatusMessage.Message.Add("Sistem verileri alýnýyor....");
            await Startup.EnsureSistemDbAsync();            
        }
        private async void LoadDataAsync()
        {
           
            var activationInfo = ActivationInfo.CreateDefault();

            await Startup.ConfigureAsync();

            var shellArgs = new ShellArgs
            {
                ViewModel = activationInfo.EntryViewModel,
                Parameter = activationInfo.EntryArgs,
                UserInfo = KullaniciModel.Default
            };
            rootFrame.Navigate(typeof(LoginView), shellArgs);
            App.MainWindow.Content = rootFrame;
            var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();

            if (rootFrame is FrameworkElement element)
            {
                element.RequestedTheme = themeSelectorService.Theme;
            }
        }

        private async Task ShowMessage(string message)
        {
            DispatcherQueue.TryEnqueue(
                () =>
                {
                    LoadingText.Text = message;
                });

            // Bir süre sonra mesajý kaybet
            await SlideOutMessage(LoadingText);
            await Task.Delay(400);
        }

        private async Task SlideOutMessage(TextBlock textBlock)
        {
            LoadingText = textBlock;
            var fadeOutAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400) };


            var fadeStoryboard = new Storyboard();
            Storyboard.SetTarget(fadeOutAnimation, textBlock);
            Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
            fadeStoryboard.Children.Add(fadeOutAnimation);
            fadeStoryboard.Begin();
            // Animasyon bitince elementi kaldýr
            await Task.Delay(400);
        }

        private async Task HideSplashScreen()
        {
            // Splash screen kaybolma animasyonu
            var fadeOutAnimation = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(500) };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(fadeOutAnimation, SplashOverlay);
            Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");

            storyboard.Children.Add(fadeOutAnimation);
            storyboard.Completed += (s, e) => SplashOverlay.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            storyboard.Begin();
            await Task.Delay(500);
            LoadingText.Text = string.Empty;
            StatusMessage.Message.Clear();
        }
    
    }
}
