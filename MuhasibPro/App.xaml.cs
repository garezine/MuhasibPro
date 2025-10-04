using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Muhasebe.Data.DatabaseManager.SistemDatabase;
using Muhasebe.Data.HostBuilders;
using MuhasibPro.Contracts.BaseAppServices;
using MuhasibPro.Contracts.SistemServices.DatabaseServices;
using MuhasibPro.Helpers;
using MuhasibPro.HostBuilders;
using System.Diagnostics;
using Velopack;
using WinUIEx;

namespace MuhasibPro
{
    public partial class App : Application
    {
        private readonly IHost _host;
        private bool _isInitialized = false;
        private DispatcherQueue _dispatcherQueue;

        public App()
        {
            this.InitializeComponent();

            // Kültür ayarı
            var culture = new System.Globalization.CultureInfo("tr-TR");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Host builder
            _host = CreateHostBuilder().Build();
            Ioc.Default.ConfigureServices(_host.Services);

            // Unhandled exception handler - UYGULAMAYI KAPATMAYI ENGELLE
            this.UnhandledException += OnUnhandledException;

            // Velopack
            VelopackApp.Build()
                .OnFirstRun(v =>
                {
                    // Show welcome message or perform first-run actions
                })
                .OnRestarted(v =>
                {
                    // Restart actions
                })
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .AddConfiguration()
                .AddDatabaseManager()
                .AddRepositories()
                .AddSystemLog()
                .AddBaseServices()
                .AddServices()
                .AddAppViewModel()
                .AddAppView();
        }

        public static UIElement? AppTitlebar { get; set; }
        public static WindowEx? MainWindow { get; } = new MainWindow();

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // DispatcherQueue'yu al
                _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                // 1. ÖNCE ARAYÜZÜ HEMEN GÖSTER
                await ActivateAsync(args);

                // 2. SONRA ARKA PLANDA DATABASE İŞLEMLERİNİ BAŞLAT
                _ = InitializeApplicationAsync();
            }
            catch (Exception ex)
            {
                // 3. KRİTİK: ASLA Exit() ÇAĞIRMA!
                await ShowStartupErrorAsync(ex, isFatal: false);

                // 4. HATA OLSA BİLE ARAYÜZÜ GÖSTERMEYE DEVAM ET
                await ActivateAsync(args);
            }
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                if (_isInitialized) return;

                _isInitialized = true;

                // 1. DATABASE INIT (BLOKLAMAYAN, GÜVENLİ VERSİYON)
                var databaseSuccess = await InitializeSystemDatabaseAsync();

                if (!databaseSuccess)
                {
                    // DATABASE HATASI - UYGULAMAYI KAPATMA, SADECE BİLDİR
                    await ShowNotificationAsync(
                        "Veritabanı başlatılırken bazı sorunlar oluştu. " +
                        "Uygulama çalışmaya devam edecek ancak bazı özellikler kısıtlı olabilir.",
                        "Bilgi");
                }

                // 2. SİSTEM SERVİSLERİNİ KONTROL ET (NON-BLOCKING)
                await CheckSystemServicesAsync();

                // 3. TEMA AYARLARINI UYGULA
                await SetThemeAsync();

                Debug.WriteLine("Application initialization completed successfully");
            }
            catch (Exception ex)
            {
                // 4. HATA YÖNETİMİ - UYGULAMAYI ASLA KAPATMA!
                Debug.WriteLine($"Application initialization completed with warnings: {ex.Message}");

                await ShowNotificationAsync(
                    "Uygulama başlatılırken bazı sorunlar oluştu, " +
                    "ancak temel işlevlerle çalışmaya devam edebilirsiniz.",
                    "Uyarı");
            }
        }

        private async Task<bool> InitializeSystemDatabaseAsync()
        {
            try
            {
                var systemService = _host.Services.GetRequiredService<ISistemDatabaseService>();
                var success = await systemService.ApplyDatabaseUpdatesAsync();

                if (!success)
                {
                    Debug.WriteLine("Database initialization completed with warnings");

                    // FALLBACK: Database servisi kullanılamazsa direkt manager'ı dene
                    return await TryFallbackDatabaseInitializationAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization failed: {ex.Message}");

                // FALLBACK mekanizması
                return await TryFallbackDatabaseInitializationAsync();
            }
        }

        private async Task<bool> TryFallbackDatabaseInitializationAsync()
        {
            try
            {
                // Direkt database manager ile dene
                var databaseManager = _host.Services.GetRequiredService<ISistemDatabaseManager>();
                return await databaseManager.InitializeDatabaseAsync();
            }
            catch (Exception fallbackEx)
            {
                Debug.WriteLine($"Fallback database initialization also failed: {fallbackEx.Message}");
                return false; // Başarısız ama uygulama yine de çalışsın
            }
        }

        private async Task CheckSystemServicesAsync()
        {
            try
            {
                var sistemService = Ioc.Default.GetRequiredService<ISistemDatabaseService>();
                var systemStatus = await sistemService.GetSystemStatusAsync();
                Debug.WriteLine($"System Status: {systemStatus}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"System service check failed: {ex.Message}");
                // Servis hatası uygulamayı durdurmamalı
            }
        }

        private async Task SetThemeAsync()
        {
            try
            {
                // DispatcherQueue ile UI thread'de çalış
                if (_dispatcherQueue != null)
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
                        if (MainWindow?.Content is FrameworkElement rootElement && themeSelectorService != null)
                        {
                            rootElement.RequestedTheme = themeSelectorService.Theme;
                        }                
                    });
                }
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Theme setting failed: {ex.Message}");
            }
        }

        private async Task ActivateAsync(LaunchActivatedEventArgs args)
        {
            try
            {
                WindowHelper.SetMainWindow(MainWindow);

                var activationService = Ioc.Default.GetService<IActivationService>();
                if (activationService != null)
                {
                    await activationService.ActivateAsync(args);
                }
                else
                {
                    // Activation service yoksa bile pencereyi göster
                    MainWindow?.Activate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Activation failed: {ex.Message}");

                // Activation başarısız olsa bile pencereyi göster
                MainWindow?.Activate();
            }
        }

        private async Task ShowNotificationAsync(string message, string title = "Bilgi")
        {
            try
            {
                // UI thread'de çalıştır
                if (_dispatcherQueue != null)
                {
                    var taskCompletionSource = new TaskCompletionSource<bool>();

                    _dispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            if (MainWindow?.Content?.XamlRoot != null)
                            {
                                var dialog = new ContentDialog
                                {
                                    Title = title,
                                    Content = message,
                                    PrimaryButtonText = "Tamam",
                                    XamlRoot = MainWindow.Content.XamlRoot
                                };
                                await dialog.ShowAsync();
                            }
                            taskCompletionSource.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Notification failed: {ex.Message}");
                            taskCompletionSource.SetResult(false);
                        }
                    });

                    await taskCompletionSource.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }

        private async Task ShowStartupErrorAsync(Exception ex, bool isFatal = false)
        {
            Debug.WriteLine($"Startup error: {ex}");

            // DEBUG modda detaylı hata göster, RELEASE'de basit mesaj
#if DEBUG
            await ShowNotificationAsync(
                $"Uygulama başlatılırken hata oluştu:\n{ex.Message}\n\n" +
                "Uygulama çalışmaya devam edecek ancak bazı özellikler kısıtlı olabilir.",
                "Başlangıç Hatası");
#else
            await ShowNotificationAsync(
                "Uygulama başlatılırken beklenmedik bir hata oluştu. " +
                "Çalışmaya devam edebilirsiniz, ancak sorun devam ederse uygulamayı yeniden başlatın.",
                "Sistem Uyarısı");
#endif
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // KRİTİK: Exception'ı handled olarak işaretle - uygulama çökmeyecek
            e.Handled = true;

            Debug.WriteLine($"Unhandled exception (handled): {e.Exception}");

            // UI thread'de hata göster
            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowNotificationAsync(
                        "Beklenmedik bir hata oluştu, ancak uygulama çalışmaya devam ediyor.",
                        "Sistem Hatası");
                });
            }
        }
    }
}