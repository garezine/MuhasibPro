using Muhasib.Business.Services.Contracts.BaseServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Settings;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class MainMenuViewModel : ShellViewModel
    {
        public MainMenuViewModel(IAuthenticationService authenticationService, ICommonServices commonServices) : base(
            authenticationService,
            commonServices)
        {
        }


        private ObservableCollection<NavigationItem> _navigationItems;

        public ObservableCollection<NavigationItem> NavigationItems
        {
            get => _navigationItems;
            set => Set(ref _navigationItems, value);
        }

        public void InitializeNavigationItems()
        {
            NavigationItems = new ObservableCollection<NavigationItem>
            {
                // Ana sayfa
                new NavigationItem(0xE80F, "Ana Sayfa", typeof(DashboardViewModel)),

                // Muhasebe menüsü (alt menülerle)
                new NavigationItem(0xE8B7, "Muhasebe", null)
                {
                    Children =
                        {
                            new NavigationItem(0xE8EC, "Gelir Gider", typeof(Nullable)),
                            new NavigationItem(0xE8C7, "Faturalar", typeof(Nullable)),
                            new NavigationItem(0xE8AB, "Raporlar", typeof(Nullable))
                        }
                },

                // Stok yönetimi
                new NavigationItem(0xE7B8, "Stok", null)
                {
                    Children =
                        {
                            new NavigationItem(0xE8F1, "Ürün Listesi", typeof(Nullable)),
                            new NavigationItem(0xE8C8, "Stok Hareketleri", typeof(Nullable)),
                            new NavigationItem(0xE8B0, "Envanter", typeof(Nullable))
                        }
                },
                // Müşteri yönetimi
                new NavigationItem(0xE716, "Firmalar", typeof(FirmalarViewModel)),

                // Ayarlar
                new NavigationItem(0xE713, "Ayarlar", typeof(SettingsViewModel))
            };
        }
    }
}
