using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Media.Animation;
using Muhasebe.Business.Helpers;
using Muhasebe.Business.Models.SistemModel;
using MuhasibPro.App.Configuration;
using MuhasibPro.App.Services;
using MuhasibPro.App.Views.Login;
using MuhasibPro.App.Views.Splash;
using MuhasibPro.Core.Services;
using MuhasibPro.ViewModels.ViewModel.Shell;

namespace MuhasibPro.App.Views.Splash
{
    public sealed partial class ExtendedSplash : Page
    {
        private readonly Frame rootFrame;
        private bool _isProcessingComplete = false;
        public static Queue<string> StatusMessages { get; } = new Queue<string>();

        public ExtendedSplash()
        {
            InitializeComponent();
            rootFrame = new Frame();
            App.MainWindow.IsTitleBarVisible = false;
            this.Loaded += OnPageLoaded;
        }
        public string Version => ProcessInfoHelper.Version;
        public string CopyrightText
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                return $"© {currentYear} MuhasibPro";
            }
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await ShowSplashAnimation();

            // Yeni animasyonları başlat
            await Task.Delay(100); // Küçük bir gecikme ekleyin
            FolderWaveStoryboard?.Begin(); // Klasör domino animasyonu
            DoubleLayerDotsStoryboard?.Begin(); // Çift katlı elips animasyonu

            _ = Task.Run(async () => await ProcessMessagesAsync());
            _ = Task.Run(async () => await StartApplicationAsync());
        }
     
        private async Task ShowSplashAnimation()
        {
            try
            {
                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(800)
                };
                var storyboard = new Storyboard();
                Storyboard.SetTarget(fadeAnimation, SplashOverlay);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                storyboard.Children.Add(fadeAnimation);
                storyboard.Begin();

                await Task.Delay(800);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splash animation error: {ex.Message}");
            }
        }

        private async Task ProcessMessagesAsync()
        {
            while (!_isProcessingComplete)
            {
                try
                {
                    if (StatusMessages.Count > 0)
                    {
                        var message = StatusMessages.Dequeue();
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            ShowStatus(message);
                        });
                        await Task.Delay(800);
                    }
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Message processing error: {ex.Message}");
                }
            }
        }

        private void ShowStatus(string statusText)
        {
            try
            {
                if (LoadingText != null)
                {
                    LoadingText.Text = statusText;
                    LoadingText.Opacity = 1;
                    System.Diagnostics.Debug.WriteLine($"Status: {statusText}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Status display error: {ex.Message}");
            }
        }

        private async Task StartApplicationAsync()
        {
            try
            {
                StatusMessages.Enqueue("Sistem başlatılıyor...");
                await Task.Delay(400);
                StatusMessages.Enqueue("Yapılandırma yükleniyor...");
                await Startup.Instance.ConfigureAsync();
                await Task.Delay(400);
                StatusMessages.Enqueue("Veritabanı kontrol ediliyor..."); 
                await Startup.Instance.EnsureSistemDbAsync();
                await Task.Delay(400);
                StatusMessages.Enqueue("Servisler hazırlanıyor...");
                await Task.Delay(400);
                StatusMessages.Enqueue("Klasörler düzenleniyor...");
                await Task.Delay(400);
                StatusMessages.Enqueue("Hazır!");
                await Task.Delay(400);

                if (!_isProcessingComplete)
                {
                    _isProcessingComplete = true;
                    await CompleteInitializationAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application startup error: {ex.Message}");
                StatusMessages.Enqueue($"Hata: {ex.Message}");
                await Task.Delay(3000);
                if (!_isProcessingComplete)
                {
                    _isProcessingComplete = true;
                    await CompleteInitializationAsync();
                }
            }
        }

        private async Task CompleteInitializationAsync()
        {
            try
            {
                await HideSplash();
                await LoadMainApplicationAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Complete initialization error: {ex.Message}");
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    SplashOverlay.Visibility = Visibility.Collapsed;
                    App.MainWindow.IsTitleBarVisible = true;
                });
            }
        }

        private async Task LoadMainApplicationAsync()
        {
            try
            {                
                var activationInfo = ActivationInfo.CreateDefault();
                var shellArgs = new ShellArgs
                {
                    ViewModel = activationInfo.EntryViewModel,
                    Parameter = activationInfo.EntryArgs,
                    UserInfo = KullaniciModel.Default
                };
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    rootFrame.Navigate(typeof(LoginView), shellArgs);
                    App.MainWindow.Content = rootFrame;
                    var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
                    if (rootFrame is FrameworkElement element)
                    {
                        element.RequestedTheme = themeSelectorService.Theme;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load main application error: {ex.Message}");
            }
        }

        private async Task HideSplash()
        {
            try
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    var fadeAnimation = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(500) };
                    var storyboard = new Storyboard();
                    Storyboard.SetTarget(fadeAnimation, SplashOverlay);
                    Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                    storyboard.Children.Add(fadeAnimation);
                    storyboard.Completed += (s, e) =>
                    {
                        SplashOverlay.Visibility = Visibility.Collapsed;
                        App.MainWindow.IsTitleBarVisible = true;
                    };
                    storyboard.Begin();
                });
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide splash error: {ex.Message}");
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    SplashOverlay.Visibility = Visibility.Collapsed;
                    App.MainWindow.IsTitleBarVisible = true;
                });
            }
        }
    }

    public static class DispatcherQueueExtensions
    {
        public static async Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.TryEnqueue(() =>
            {
                try { action(); tcs.SetResult(true); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            await tcs.Task;
        }
    }
}
