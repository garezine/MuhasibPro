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
using NuGet.Versioning;
using System.Diagnostics;
using Velopack;
using WinUIEx;

namespace MuhasibPro
{
    public partial class App : Application
    {
        private readonly IHost _host;
        private int _isInitialized = 0; // Thread-safe flag (0 = false, 1 = true)
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

            // Unhandled exception handler
            this.UnhandledException += OnUnhandledException;

            // Velopack - Non-blocking initialization
            try
            {
                VelopackApp.Build()
                    .OnFirstRun(OnVelopackFirstRun)
                    .OnRestarted(OnVelopackRestarted)
                    .Run();
            }
            catch (Exception ex)
            {
                // Velopack hatası uygulamayı durdurmamalı
                Debug.WriteLine($"Velopack initialization failed: {ex.Message}");
            }
        }

        private void OnVelopackFirstRun(SemanticVersion v)
        {
            Debug.WriteLine($"First run detected: v{v}");
            // İlk çalıştırma işlemleri
        }

        private void OnVelopackRestarted(SemanticVersion v)
        {
            Debug.WriteLine($"Application restarted after update: v{v}");
            // Güncelleme sonrası işlemler
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
                Debug.WriteLine($"OnLaunched error: {ex}");

                // ASLA Exit() ÇAĞIRMA!
                await ShowStartupErrorAsync(ex, isFatal: false);

                // HATA OLSA BİLE ARAYÜZÜ GÖSTER
                try
                {
                    await ActivateAsync(args);
                }
                catch (Exception activateEx)
                {
                    Debug.WriteLine($"Activation fallback failed: {activateEx}");
                    // Son çare: Pencereyi direkt göster
                    MainWindow?.Activate();
                }
            }
        }

        private async Task InitializeApplicationAsync()
        {
            // Thread-safe initialization check
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
            {
                Debug.WriteLine("Application already initialized, skipping...");
                return;
            }

            try
            {
                Debug.WriteLine("Starting application initialization...");

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
                Debug.WriteLine($"Application initialization error: {ex.Message}");

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
                    return await TryFallbackDatabaseInitializationAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization failed: {ex.Message}");
                return await TryFallbackDatabaseInitializationAsync();
            }
        }

        private async Task<bool> TryFallbackDatabaseInitializationAsync()
        {
            try
            {
                var databaseManager = _host.Services.GetRequiredService<ISistemDatabaseManager>();
                return await databaseManager.InitializeDatabaseAsync();
            }
            catch (Exception fallbackEx)
            {
                Debug.WriteLine($"Fallback database initialization also failed: {fallbackEx.Message}");
                return false;
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
            }
        }

        private async Task SetThemeAsync()
        {
            try
            {
                if (_dispatcherQueue == null)
                {
                    Debug.WriteLine("DispatcherQueue not available for theme setting");
                    return;
                }

                var tcs = new TaskCompletionSource<bool>();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
                        if (MainWindow?.Content is FrameworkElement rootElement && themeSelectorService != null)
                        {
                            rootElement.RequestedTheme = themeSelectorService.Theme;
                        }
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Theme application failed: {ex.Message}");
                        tcs.SetResult(false);
                    }
                });

                await tcs.Task;
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
                    Debug.WriteLine("Activation service not found, activating window directly");
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
            if (_dispatcherQueue == null)
            {
                Debug.WriteLine($"Cannot show notification (no dispatcher): {message}");
                return;
            }

            try
            {
                var tcs = new TaskCompletionSource<bool>();

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
                        else
                        {
                            Debug.WriteLine($"Cannot show dialog (no XamlRoot): {message}");
                        }
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Notification dialog failed: {ex.Message}");
                        tcs.SetResult(false);
                    }
                });

                await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Notification failed: {ex.Message}");
            }
        }

        private async Task ShowStartupErrorAsync(Exception ex, bool isFatal = false)
        {
            Debug.WriteLine($"Startup error: {ex}");

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
            // KRİTİK: Exception'ı handled olarak işaretle
            e.Handled = true;

            Debug.WriteLine($"Unhandled exception caught and handled: {e.Exception}");

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