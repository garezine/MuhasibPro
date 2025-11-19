using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Data.Helper;
using MuhasibPro.Configurations;
using MuhasibPro.Contracts.CoreServices;
using MuhasibPro.HostBuilders;
using MuhasibPro.Services.Infrastructure.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.Views.Firma;
using MuhasibPro.Views.Login;
using System.Diagnostics;
using Windows.UI;

namespace MuhasibPro.Views.Splash
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

            _ = Task.Run(async () => await ProcessMessagesAsync());
            _ = Task.Run(async () => await StartApplicationAsync());
        }

        private async Task ShowSplashAnimation()
        {
            try
            {
                var fadeAnimation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(800) };
                var storyboard = new Storyboard();
                Storyboard.SetTarget(fadeAnimation, SplashOverlay);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                storyboard.Children.Add(fadeAnimation);
                storyboard.Begin();
                await Task.Delay(400);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splash animation error: {ex.Message}");
            }
        }

        private async Task ProcessMessagesAsync()
        {
            while(!_isProcessingComplete)
            {
                try
                {
                    if(StatusMessages.Count > 0)
                    {
                        var message = StatusMessages.Dequeue();
                        await DispatcherQueue.EnqueueAsync(
                            () =>
                            {
                                ShowStatus(message);
                            });
                        await Task.Delay(100);
                    }
                } catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Message processing error: {ex.Message}");
                }
            }
        }

        private void ShowStatus(string statusText)
        {
            try
            {
                if(LoadingText != null)
                {
                    LoadingText.Text = statusText;
                    LoadingText.Opacity = 0.5;
                    System.Diagnostics.Debug.WriteLine($"Status: {statusText}");
                }
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Status display error: {ex.Message}");
            }
        }


        private async Task StartApplicationAsync()
        {
            try
            {
                await ExecuteStep("Sistem başlatılıyor...", 400);
                await ExecuteStep("Klasörler düzenleniyor...", 400);
                await ExecuteStep("Servisler hazırlanıyor...", 400);

                await ExecuteStep("Yapılandırma yükleniyor...", 400, Startup.Instance.ConfigureAsync);
                await ExecuteStep("Veritabanı kontrol ediliyor...", 400);
                var dbTest = await Startup.Instance.SistemDatabaseConnectionTest();
                if(!dbTest)
                {
                    await ShowNotificationAsync(
                        "Veritabanı başlatılırken bazı sorunlar oluştu. " +
                            "Uygulama başlatılacak, ancak uygulamada ani kapanma ve veritabanı sorunları yaşanabilir.",
                        "Bilgi");
                }

                await ExecuteStep("Hazır!", 400);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application startup error: {ex.Message}");
                StatusMessages.Enqueue($"Hata: {ex.Message}");
                await Task.Delay(3000);
            } finally
            {
                await Task.Delay(500);
                await MarkProcessingCompleteAsync();
            }
        }

        private async Task ExecuteStep(string message, int delay, Func<Task>? asyncAction = null)
        {
            StatusMessages.Enqueue(message);
            await Task.Delay(delay);

            if(asyncAction != null)
                await asyncAction();
        }


        private async Task MarkProcessingCompleteAsync()
        {
            if(!_isProcessingComplete)
            {
                _isProcessingComplete = true;
                await CompleteInitializationAsync();
            }
        }

        private async Task CompleteInitializationAsync()
        {
            try
            {
                await HideSplash();
                await LoadMainApplicationAsync();
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Complete initialization error: {ex.Message}");
                await DispatcherQueue.EnqueueAsync(
                    () =>
                    {
                        SplashOverlay.Visibility = Visibility.Collapsed;
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
                await DispatcherQueue.EnqueueAsync(
                    () =>
                    {
                        rootFrame.Navigate(typeof(LoginView), shellArgs);
                        App.MainWindow.Content = rootFrame;
                        var themeSelectorService = ServiceLocator.Current.GetService<IThemeSelectorService>();
                        if(rootFrame is FrameworkElement element)
                        {
                            element.RequestedTheme = themeSelectorService.Theme;
                        }
                    });
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load main application error: {ex.Message}");
            }
        }

        private async Task HideSplash()
        {
            try
            {
                await DispatcherQueue.EnqueueAsync(
                    () =>
                    {
                        var fadeAnimation = new DoubleAnimation
                        {
                            From = 1,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(500)
                        };
                        var storyboard = new Storyboard();
                        Storyboard.SetTarget(fadeAnimation, SplashOverlay);
                        Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                        storyboard.Children.Add(fadeAnimation);
                        storyboard.Completed += (s, e) =>
                        {
                            SplashOverlay.Visibility = Visibility.Collapsed;
                        };
                        storyboard.Begin();
                    });
                await Task.Delay(500);
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide splash error: {ex.Message}");
                await DispatcherQueue.EnqueueAsync(
                    () =>
                    {
                        SplashOverlay.Visibility = Visibility.Collapsed;
                    });
            }
        }

        private async Task ShowNotificationAsync(string message, string title = "Bilgi")
        {
            try
            {
                {
                    var taskCompletionSource = new TaskCompletionSource<bool>();
                    DispatcherQueue.TryEnqueue(
                        async () =>
                        {
                            try
                            {
                                if(this?.Content?.XamlRoot != null)
                                {
                                    var dialog = new ContentDialog
                                    {
                                        Title = title,
                                        Content = message,
                                        PrimaryButtonText = "Tamam",
                                        XamlRoot = this.Content.XamlRoot
                                    };
                                    await dialog.ShowAsync();
                                }
                                taskCompletionSource.SetResult(true);
                            } catch(Exception ex)
                            {
                                Debug.WriteLine($"Notification failed: {ex.Message}");
                                taskCompletionSource.SetResult(false);
                            }
                        });
                    await taskCompletionSource.Task;
                }
            } catch(Exception ex)
            {
                Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }
    }
}


