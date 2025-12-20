using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.AppServices;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.DatabaseServices.TenantDatabase;
using Muhasib.Business.Services.Contracts.UIServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.ViewModels.Shell;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.Firmalar;
using MuhasibPro.ViewModels.ViewModels.SistemViewModel.MaliDonemler;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Dashboard
{
    public class FirmaWithMaliDonemSelectViewModel : FirmalarViewModel
    {
        public IAuthenticationService AuthenticationService { get; }

        public ILocalSettingsService LocalSettingsService { get; }

        public IFirmaWithMaliDonemSelectedService SelectedFirmaService { get; }

        public ITenantSQLiteWorkflowService TenantWorkflowService { get; }


        public MaliDonemViewModel MaliDonemViewModel { get; }

        public FirmaWithMaliDonemSelectViewModel(
            ICommonServices commonServices,
            IFirmaService firmaService,
            IFilePickerService filePickerService,
            IMaliDonemService maliDonemService,
            ILocalSettingsService localSettingsService,
            IFirmaWithMaliDonemSelectedService selectedFirmaService,
            IAuthenticationService authenticationService,
            ITenantSQLiteWorkflowService tenantWorkflowService) : base(
            commonServices,
            firmaService,
            filePickerService,
            maliDonemService)
        {
            LocalSettingsService = localSettingsService;
            SelectedFirmaService = selectedFirmaService;

            AuthenticationService = authenticationService;
            TenantWorkflowService = tenantWorkflowService;
            MaliDonemViewModel = new MaliDonemViewModel(commonServices, maliDonemService, tenantWorkflowService);
            DevamEtCommand = new AsyncRelayCommand(ExecuteDevamEt, CanExecuteDevamEt);

            PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == nameof(HasSelection))
                {
                    (DevamEtCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            };
        }

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
                    NotifyPropertyChanged(nameof(SecimOzeti));
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
                    NotifyPropertyChanged(nameof(VeritabaniBilgisi));
                    NotifyPropertyChanged(nameof(VeritabaniBaglantiDurumu));
                    NotifyPropertyChanged(nameof(SecimOzeti));
                    _ = CheckDatabaseHealthAsync();
                }
            }
        }

        // UI State Properties
        public bool IsFirmaSelected => SelectedFirma != null;

        public bool IsMaliDonemSelected => SelectedMaliDonem != null;

        public bool HasSelection => IsFirmaSelected && IsMaliDonemSelected;

        // Özet Panel Properties
        public string SecimOzeti => HasSelection
            ? "Seçiminiz tamamlandı, devam edebilirsiniz"
            : "Lütfen firma ve mali dönem seçiniz";

        public string FirmaBilgisi => IsFirmaSelected ? $"{SelectedFirma.KisaUnvani}" : "Firma seçilmedi";

        public string DonemBilgisi => IsMaliDonemSelected
            ? $"{SelectedMaliDonem.MaliYil} Mali Dönemi"
            : "Dönem seçilmedi";

        public string VeritabaniBilgisi => IsMaliDonemSelected
            ? $"{SelectedMaliDonem.DatabaseType} - {SelectedMaliDonem.DBName}"
            : "";

        private string _veritabaniBaglantiDurumu = "Bağlantı durumu bekleniyor...";

        public string VeritabaniBaglantiDurumu
        {
            get => _veritabaniBaglantiDurumu;
            private set => Set(ref _veritabaniBaglantiDurumu, value);
        }

        public string KullaniciBilgisi => AuthenticationService.IsAuthenticated
            ? $"{AuthenticationService.CurrentAccount?.Adi} {AuthenticationService.CurrentAccount?.Soyadi}"
            : "Bilinmeyen Kullanıcı";
        #endregion

        #region Commands
        private bool CanExecuteDevamEt() { return HasSelection; }

        public ICommand DevamEtCommand { get; }
        #endregion

        #region Load &Initialize
        public async Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            try
            {
                await base.LoadAsync(new FirmaListArgs());
                if(FirmaList.Items == null || FirmaList.Items.Count == 0)
                {
                    await HandleNoFirmaFound();
                    return;
                }
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
                    var lastFirma = FirmaList.Items.FirstOrDefault(f => f.Id == lastFirmaId);
                    if(lastFirma != null)
                    {
                        // Firma seçimini otomatik yap
                        FirmaList.SelectedItem = lastFirma;
                        base.OnItemSelected();

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
            } finally
            {
                IsBusy = false;
            }
        }

        public void BaseSubscribe()
        {
            MessageService.Subscribe<FirmaListViewModel>(this, OnMessage);
            MessageService.Subscribe<MaliDonemListViewModel>(this, OnMaliDonemMessage);
            FirmaList.Subscribe();
            FirmaDetails.Subscribe();
        }

        public void BaseUnsubscribe() { base.Unsubscribe(); }

        private async void OnMessage(FirmaListViewModel viewModel, string message, object args)
        {
            if(viewModel == FirmaList && message == "ItemSelected")
            {
                await ContextService.RunAsync(
                    () =>
                    {
                        OnItemSelected();
                        SelectedFirma = FirmaList.SelectedItem;
                    });
            }
        }

        private async void OnMaliDonemMessage(MaliDonemListViewModel viewModel, string message, object args)
        {
            if(viewModel == MaliDonemList && message == "ItemSelected")
            {
                await ContextService.RunAsync(
                    () =>
                    {
                        MaliDonemViewModel.OnItemSelected();
                        SelectedMaliDonem = MaliDonemList.SelectedItem;
                        NotifyPropertyChanged(nameof(HasSelection));
                    });
            }
        }
        #endregion

        #region Command Handlers
        private async Task ExecuteDevamEt()
        {
            if(!HasSelection)
                return;           
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
                SelectedFirmaService.SelectedFirma = SelectedFirma;
                SelectedFirmaService.SelectedMaliDonem = SelectedMaliDonem;

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

        private async Task SwitchToTenantAndUpdateState(string databaseName, FirmaModel firma, MaliDonemModel maliDonem)
        {
            // 1. Tenant'a bağlan (DI güncellenir)
            var result = await TenantWorkflowService.SwitchTenantAsync(databaseName);

            // 2. Application State'i güncelle
            SelectedFirmaService.SelectedFirma = firma;
            SelectedFirmaService.SelectedMaliDonem = maliDonem;
            SelectedFirmaService.ConnectedTenantDb = result.Data;

            // 3. UI güncellenir (StateChanged event otomatik fire olur)
        }

        private async Task CheckDatabaseHealthAsync()
        {
            if(SelectedMaliDonem == null)
            {
                VeritabaniBaglantiDurumu = "Mali dönem seçilmedi";
                return;
            }

            VeritabaniBaglantiDurumu = "Kontrol ediliyor...";

            try
            {
                var result = await TenantWorkflowService.GetHealthStatusAsync(SelectedMaliDonem.DBName);

                VeritabaniBaglantiDurumu = result.Success ? result.Data.HealthStatus : "Bağlantı başarısız";
            } catch
            {
                VeritabaniBaglantiDurumu = "Bağlantı hatası";
            }
        }
        #endregion
    }
}