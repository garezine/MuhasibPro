using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Muhasebe.Business.HostBuilders;
using Muhasebe.Data.Database.Interfaces.Services;
using Muhasebe.Domain.Helpers;
using MuhasibPro.Core.Infrastructure.Helpers;
using MuhasibPro.Core.Services;
using MuhasibPro.HostBuilders;

namespace MuhasibPro
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            InitializeComponent();
            StatusMessage.Message.Add("Uygulama başlatılıyor...");
            _host = CreateHostBuilder().Build();

            StatusMessage.Message.Add("Servisler çalıştırılıyor...");
            Ioc.Default.ConfigureServices(_host.Services);
        }
        public static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .AddConfiguration()
                .AddDatabaseManager()
                .AddRepository()
                .AddServices()
                .AddSystemLog()
                .UseContentRoot(AppContext.BaseDirectory)
                .AddAppServices()
                .AddAppViewModel()
                .AddAppView();
        }
        public static WindowEx MainWindow { get; } = new MainWindow();
        public static UIElement? AppTitlebar { get; set; }

        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var databaseManager = Ioc.Default.GetService<IDatabaseRestoreService>();
            databaseManager.RestoreUpdateSistemDatabase();

            var themeSelectorService = Ioc.Default.GetService<IThemeSelectorService>();
            if (MainWindow.Content is FrameworkElement rootelement)
            {
                rootelement.RequestedTheme = themeSelectorService.Theme;
            }        
            await ActivateAsync(args);
        }
        private async Task ActivateAsync(LaunchActivatedEventArgs args)
        {
            WindowHelper.SetMainWindow(MainWindow);

            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }
   


    }
}
