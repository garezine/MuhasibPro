using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using Muhasib.Domain.Entities.SistemEntity;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Loggings.SistemLogs;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class MainShellViewModel : MainMenuViewModel
    {
        public MainShellViewModel(IAuthenticationService authenticationService, ICommonServices commonServices) : base(authenticationService, commonServices)
        {
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }
        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, value);
        }

        public override async Task LoadAsync(ShellArgs args)
        {
            InitializeNavigationItems();
            //await UpdateAppLogBadge();
            await base.LoadAsync(args);
        }

        public override void Subscribe()
        {
            MessageService.Subscribe<ILogService, AppLog>(this, OnLogServiceMessage);
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        public override void Unload()
        {
            base.Unload();
        }

        
        public async void NavigateTo(Type viewModel)
        {
            StatusReady();
            if (viewModel != null)
            {
                switch (viewModel.Name)
                {
                    case "DashboardViewModel":
                        NavigationService.Navigate(viewModel);
                        break;
                    case "CustomersViewModel":
                        //NavigationService.Navigate(viewModel, new CustomerListArgs());
                        break;
                    case "OrdersViewModel":
                        //NavigationService.Navigate(viewModel, new OrderListArgs());
                        break;
                    case "FirmalarViewModel":
                        NavigationService.Navigate(viewModel, new FirmaListArgs());
                        break;
                    case "SistemLogsViewModel":
                        NavigationService.Navigate(viewModel, new SistemLogListArgs());
                        await LogService.SistemLogService.SistemLogMarkAllAsReadAsync();
                        //await UpdateAppLogBadge();
                        break;
                    case "SettingsViewModel":
                        NavigationService.Navigate(viewModel);
                        break;
                    default:
                        NavigationService.Navigate<DashboardViewModel>();
                        break;
                }
            }
            else
            {
                // Hata durumunda ana sayfaya yönlendir
                await DialogService.ShowAsync("Bilgi", "Henüz bu sayfalar hazırlanmadı");
            }
        }

        private async void OnLogServiceMessage(ILogService logService, string message, AppLog log)
        {
            if (message == "LogAdded")
            {
                await ContextService.RunAsync(async () =>
                {
                    await UpdateAppLogBadge();
                });
            }
        }

        private async Task UpdateAppLogBadge()
        {
            int count = await LogService.SistemLogService.GetSistemLogsCountAsync(new DataRequest<SistemLog> { Where = r => !r.IsRead });
            //AppLogsItem.Badge = count > 0 ? count.ToString() : null;
        }
    }


}


