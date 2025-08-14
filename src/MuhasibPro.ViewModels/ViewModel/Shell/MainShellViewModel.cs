using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Entities.Uygulama;
using MuhasibPro.Core.Models;
using MuhasibPro.Core.Services.Common;
using MuhasibPro.ViewModels.ViewModel.Dashboard;
using MuhasibPro.ViewModels.ViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModel.Settings;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModel.Shell
{
    public class MainShellViewModel : ShellViewModel
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

        private ObservableCollection<NavigationItem> _navigationItems;
        public ObservableCollection<NavigationItem> NavigationItems
        {
            get => _navigationItems;
            set => Set(ref _navigationItems, value);
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

        private void InitializeNavigationItems()
        {
            NavigationItems = new ObservableCollection<NavigationItem>
        {
            // Ana sayfa
            new NavigationItem(0xE80F, "Ana Sayfa", typeof(DashboardViewModel)),
            
            // Muhasebe menüsü (alt menülerle)
            new NavigationItem(0xE8B7, "Muhasebe", null,
                new ObservableCollection<NavigationItem>
                {
                    new NavigationItem(0xE8EC, "Gelir Gider", typeof(Nullable)),
                    new NavigationItem(0xE8C7, "Faturalar", typeof(Nullable)),
                    new NavigationItem(0xE8AB, "Raporlar", typeof(Nullable))
                }),
            
            // Stok yönetimi
            new NavigationItem(0xE7B8, "Stok", null,
                new ObservableCollection<NavigationItem>
                {
                    new NavigationItem(0xE8F1, "Ürün Listesi", typeof(Nullable)),
                    new NavigationItem(0xE8C8, "Stok Hareketleri", typeof(Nullable)),
                    new NavigationItem(0xE8B0, "Envanter", typeof(Nullable))
                }),
            
            // Müşteri yönetimi
            new NavigationItem(0xE716, "Firmalar", typeof(FirmalarViewModel)),
            
            // Ayarlar
            new NavigationItem(0xE713, "Ayarlar", typeof(SettingsViewModel))
        };

            // Badge örnekleri
            NavigationItems[1].Badge = "5"; // Muhasebe menüsüne badge
            NavigationItems[1].Children[1].Badge = "2"; // Faturalar alt menüsüne badge
        }
        public async void NavigateTo(Type viewModel)
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
                case "AppLogsViewModel":
                    //NavigationService.Navigate(viewModel, new AppLogListArgs());
                    await LogService.MarkAllAsReadAsync();
                    //await UpdateAppLogBadge();
                    break;
                case "SettingsViewModel":
                    NavigationService.Navigate(viewModel);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        
        private async void OnLogServiceMessage(ILogService logService, string message, AppLog log)
        {
            if (message == "LogAdded")
            {
                await ContextService.RunAsync(async () =>
                {
                    //await UpdateAppLogBadge();
                });
            }
        }

        //private async Task UpdateAppLogBadge()
        //{
        //    int count = await LogService.GetLogsCountAsync(new DataRequest<AppLog> { Where = r => !r.IsRead });
        //    AppLogsItem.Badge = count > 0 ? count.ToString() : null;
        //}
    }


}


