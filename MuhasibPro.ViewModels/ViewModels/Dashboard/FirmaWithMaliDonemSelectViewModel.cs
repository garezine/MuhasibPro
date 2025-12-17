using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.AppServices;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.UIServices;
using Muhasib.Data.DataContext;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Login;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Dashboard
{
    public class FirmaWithMaliDonemSelectViewModel : ViewModelBase
    {
        #region Services & Dependencies
        public IAuthenticationService AuthenticationService { get; }
        public IFirmaWithMaliDonemSelectedService FirmaWithMaliDonemSelectedService { get; set; }
        public ITenantSQLiteConnectionService TenantSQLiteConnectionService { get; }
        public IFirmaService FirmaService { get; }

        public IMaliDonemService MaliDonemService { get; }

        public ILocalSettingsService LocalSettingsService { get; }
        #endregion

        #region Child ViewModels
        public FirmaListViewModel FirmalarList { get; }

        public MaliDonemListViewModel MaliDonemList { get; }
        #endregion

        #region Constructor
        public FirmaWithMaliDonemSelectViewModel(
            ICommonServices commonServices,
            IAuthenticationService authenticationService,
            ILocalSettingsService localSettingsService,
            IFirmaService firmaService,
            IMaliDonemService maliDonemService,
            IFirmaWithMaliDonemSelectedService firmaWithMaliDonemSelectedService,
            ITenantSQLiteConnectionService tenantSQLiteConnectionService) : base(commonServices)
        {
            AuthenticationService = authenticationService;
            FirmaService = firmaService;
            MaliDonemService = maliDonemService;
            LocalSettingsService = localSettingsService;

            // Child ViewModels
            FirmalarList = new FirmaListViewModel(commonServices, FirmaService);
            MaliDonemList = new MaliDonemListViewModel(commonServices, MaliDonemService);

            // Commands
            DevamEtCommand = new AsyncRelayCommand(ExecuteDevamEt, CanExecuteDevamEt);

            CikisYapCommand = new AsyncRelayCommand(ExecuteCikisYap);
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CanProceed))
                {
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            };
            FirmaWithMaliDonemSelectedService = firmaWithMaliDonemSelectedService;
            TenantSQLiteConnectionService = tenantSQLiteConnectionService;
        }
        #endregion

        #region Properties
        public ShellArgs ViewModelArgs { get; set; }

        private FirmaModel _selectedFirma;

        public FirmaModel SelectedFirma
        {
            get => _selectedFirma;
            private set
            {
                if(Set(ref _selectedFirma, value))
                {
                    NotifyPropertyChanged(nameof(IsFirmaSelected));
                    NotifyPropertyChanged(nameof(HasSelection));
                    NotifyPropertyChanged(nameof(FirmaBilgisi));
                    NotifyPropertyChanged(nameof(CanProceed));
                    NotifyPropertyChanged(nameof(SecimOzeti));                   
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private MaliDonemModel _selectedMaliDonem;

        public MaliDonemModel SelectedMaliDonem
        {
            get => _selectedMaliDonem;
            private set
            {
                if(Set(ref _selectedMaliDonem, value))
                {
                    NotifyPropertyChanged(nameof(IsMaliDonemSelected));
                    NotifyPropertyChanged(nameof(HasSelection));
                    NotifyPropertyChanged(nameof(DonemBilgisi));
                    NotifyPropertyChanged(nameof(VeriTabaniBilgisi));
                    NotifyPropertyChanged(nameof(CanProceed));
                    NotifyPropertyChanged(nameof(SecimOzeti));
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();                   
                }
            }
        }

        // UI State Properties
        public bool IsFirmaSelected => SelectedFirma != null;

        public bool IsMaliDonemSelected => SelectedMaliDonem != null;

        public bool HasSelection => IsFirmaSelected && IsMaliDonemSelected;

        public bool CanProceed => HasSelection && !IsBusy;

        // Özet Panel Properties
        public string SecimOzeti => HasSelection
            ? "Seçiminiz tamamlandı, devam edebilirsiniz"
            : "Lütfen firma ve mali dönem seçiniz";

        public string FirmaBilgisi => IsFirmaSelected ? $"{SelectedFirma.KisaUnvani}" : "Firma seçilmedi";

        public string DonemBilgisi => IsMaliDonemSelected
            ? $"{SelectedMaliDonem.MaliYil} Mali Dönemi"
            : "Dönem seçilmedi";

        public string VeriTabaniBilgisi => IsMaliDonemSelected
            ? $"{SelectedMaliDonem.DatabaseType} - {SelectedMaliDonem.DBName}"
            : "";

        public string KullaniciBilgisi => AuthenticationService.IsAuthenticated
            ? $"{AuthenticationService.CurrentAccount?.Adi} {AuthenticationService.CurrentAccount?.Soyadi}"
            : "Bilinmeyen Kullanıcı";
        #endregion

        #region Commands
        public ICommand DevamEtCommand { get; }

        public ICommand CikisYapCommand { get; }
        #endregion

        #region Load & Initialize
        public async Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;

            try
            {
                // Firma listesini yükle
                await FirmalarList.LoadAsync(new FirmaListArgs());

                // Eğer hiç firma yoksa
                if(FirmalarList.Items == null || FirmalarList.Items.Count == 0)
                {
                    await HandleNoFirmaFound();
                    return;
                }

                // Son seçilen firma/dönemi hatırla
                await LoadLastSelection();
            } catch(Exception ex)
            {
                await DialogService.ShowAsync("Hata", $"Veriler yüklenirken hata: {ex.Message}");
                LogSistemException("FirmaDonemSelect", "LoadAsync", ex);
            } finally
            {
                IsBusy = false;
            }
        }

        private async Task HandleNoFirmaFound()
        {
            var result = await DialogService.ShowAsync(
                "Firma Bulunamadı",
                "Henüz kayıtlı firma bulunmuyor. Yeni firma eklemek ister misiniz?",
                "Evet",
                "Hayır");

            if(result)
            {
                await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(new FirmaDetailsArgs());
            } else
            {
                await ExecuteCikisYap();
            }
        }

        private async Task LoadLastSelection()
        {
            try
            {
                // Son seçilen firma ID'sini al
                var lastFirmaId = await LocalSettingsService.ReadSettingAsync<long>("LastSelectedFirmaId");
                if(lastFirmaId > 0)
                {
                    var lastFirma = FirmalarList.Items.FirstOrDefault(f => f.Id == lastFirmaId);
                    if(lastFirma != null)
                    {
                        // Firma seçimini otomatik yap
                        FirmalarList.SelectedItem = lastFirma;
                        await OnFirmaSelected(lastFirma);

                        // Son seçilen dönemi al
                        var lastDonemId = await LocalSettingsService.ReadSettingAsync<long>("LastSelectedDonemId");
                        if(lastDonemId > 0)
                        {
                            var lastDonem = MaliDonemList.Items?.FirstOrDefault(d => d.Id == lastDonemId);
                            if(lastDonem != null)
                            {
                                MaliDonemList.SelectedItem = lastDonem;
                                SelectedMaliDonem = lastDonem;
                            }
                        }
                    }
                }
            } catch(Exception ex)
            {
                // Son seçim yüklenemezse sessizce devam et
                LogSistemException("FirmaDonemSelect", "LoadLastSelection", ex);
            }
        }

        public void Subscribe()
        {
            MessageService.Subscribe<FirmaListViewModel>(this, OnFirmaMessage);
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnMaliDonemMessage);

            FirmalarList.Subscribe();
            MaliDonemList.Subscribe();
        }

        public void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
            FirmalarList.Unsubscribe();
            MaliDonemList.Unsubscribe();
        }

        public void Unload()
        {
            FirmalarList.Unload();
            MaliDonemList.Unload();
        }
        #endregion

        #region Message Handlers
        private async void OnFirmaMessage(FirmaListViewModel viewModel, string message, object args)
        {
            if(viewModel == FirmalarList && message == "ItemSelected")
            {
                await ContextService.RunAsync(
                    async () =>
                    {
                        var selected = FirmalarList.SelectedItem;
                        if(selected != null && !selected.IsEmpty)
                        {
                            await OnFirmaSelected(selected);
                        }
                    });
            }
        }

        private async Task OnFirmaSelected(FirmaModel firma)
        {
            IsBusy = true;
            try
            {
                SelectedFirma = firma;

                // Firma detaylarını yükle
                await PopulateFirmaDetails(firma);

                // Mali dönemleri yükle
                await MaliDonemList.LoadAsync(new MaliDonemListArgs { FirmaId = firma.Id });

                // Mali dönem yoksa
                if(MaliDonemList.Items == null || MaliDonemList.Items.Count == 0)
                {
                    await HandleNoMaliDonemFound();
                } else
                {
                    // İlk dönemi otomatik seç (genelde en yeni dönem)
                    var enYeniDonem = MaliDonemList.Items.OrderByDescending(d => d.MaliYil).FirstOrDefault();
                    if(enYeniDonem != null)
                    {
                        MaliDonemList.SelectedItem = enYeniDonem;
                        SelectedMaliDonem = enYeniDonem;
                    }
                }
            } catch(Exception ex)
            {
                await DialogService.ShowAsync("Hata", $"Firma seçilirken hata: {ex.Message}");
                LogSistemException("FirmaDonemSelect", "OnFirmaSelected", ex);
            } finally
            {
                NotifyPropertyChanged(nameof(CanProceed));
                IsBusy = false;
            }
        }

        private async Task PopulateFirmaDetails(FirmaModel firma)
        {
            try
            {
                var result = await FirmaService.GetByFirmaIdAsync(firma.Id);
                if(result.Success && result.Data != null)
                {
                    firma.Merge(result.Data);
                }
            } catch(Exception ex)
            {
                LogSistemException("FirmaDonemSelect", "PopulateFirmaDetails", ex);
            }
        }

        private async Task HandleNoMaliDonemFound()
        {
            var result = await DialogService.ShowAsync(
                "Mali Dönem Bulunamadı",
                $"{SelectedFirma.KisaUnvani} firması için mali dönem bulunamadı. Yeni dönem eklemek ister misiniz?",
                "Evet",
                "Hayır");

            if(result)
            {
                await NavigationService.CreateNewViewAsync<MaliDonemDetailsViewModel>(
                    new MaliDonemDetailsArgs { FirmaId = SelectedFirma.Id });
            }
        }

        private async void OnMaliDonemMessage(MaliDonemListViewModel viewModel, string message, object args)
        {
            if(viewModel == MaliDonemList && message == "ItemSelected")
            {
                await ContextService.RunAsync(
                    () =>
                    {
                        var selected = MaliDonemList.SelectedItem;
                        if(selected != null && !selected.IsEmpty)
                        {
                            SelectedMaliDonem = selected;
                            NotifyPropertyChanged(nameof(CanProceed));
                        }
                    });
            }
        }
        #endregion

        #region Command Handlers
        private bool CanExecuteDevamEt() { return CanProceed; }

        private async Task ExecuteDevamEt()
        {
            if(!CanProceed)
                return;

            IsBusy = true;
            try
            {
                // Seçimi kaydet (bir dahaki sefere hatırlansın)
                await SaveLastSelection();

                // Log kaydet
                await LogService.SistemLogService
                    .SistemLogInformation(
                        "FirmaDonemSelect",
                        "Firma ve dönem seçildi",
                        "Seçim",
                        $"{SelectedFirma.KisaUnvani} - {SelectedMaliDonem.MaliYil}");

                // ShellArgs'ı güncelle
                FirmaWithMaliDonemSelectedService.SelectedFirma = SelectedFirma;
                FirmaWithMaliDonemSelectedService.SelectedMaliDonem = SelectedMaliDonem;

                await SwitchToTenantAndUpdateState(SelectedMaliDonem.DBName, SelectedFirma, SelectedMaliDonem);
                ViewModelArgs.ViewModel = typeof(DashboardViewModel);
                // Ana uygulamaya geç
                NavigationService.Navigate<MainShellViewModel>(ViewModelArgs);
            } catch(Exception ex)
            {
                await DialogService.ShowAsync("Hata", $"İşlem sırasında hata: {ex.Message}");
                LogSistemException("FirmaDonemSelect", "ExecuteDevamEt", ex);
            } finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveLastSelection()
        {
            try
            {
                await LocalSettingsService.SaveSettingAsync("LastSelectedFirmaId", SelectedFirma.Id);
                await LocalSettingsService.SaveSettingAsync("LastSelectedDonemId", SelectedMaliDonem.Id);
            } catch(Exception ex)
            {
                // Kaydetme hatası kritik değil, sessizce devam et
                LogSistemException("FirmaDonemSelect", "SaveLastSelection", ex);
            }
        }

        private async Task ExecuteCikisYap()
        {
            var result = await DialogService.ShowAsync(
                "Çıkış",
                "Çıkış yapmak istediğinizden emin misiniz?",
                "Evet",
                "Hayır");

            if(result)
            {
                AuthenticationService.Logout();
                NavigationService.Navigate<LoginViewModel>(new ShellArgs());
            }
        }
        #endregion
        private async Task SwitchToTenantAndUpdateState(
        string databaseName,
        FirmaModel firma,
        MaliDonemModel maliDonem)
        {
            // 1. Tenant'a bağlan (DI güncellenir)
            var result = await TenantSQLiteConnectionService.SwitchTenantAsync(databaseName);

            // 2. Application State'i güncelle
            FirmaWithMaliDonemSelectedService.SelectedFirma = firma;
            FirmaWithMaliDonemSelectedService.SelectedMaliDonem = maliDonem;
            FirmaWithMaliDonemSelectedService.ConnectedTenantDb = result.Data;

            // 3. UI güncellenir (StateChanged event otomatik fire olur)
        }
    }

}