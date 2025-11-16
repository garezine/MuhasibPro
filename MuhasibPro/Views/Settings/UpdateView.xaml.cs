using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Muhasib.Domain.Enum;
using MuhasibPro.HostBuilders;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.ViewModels.Settings;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MuhasibPro.Views.Settings
{
    public sealed partial class UpdateView : Page, INotifyPropertyChanged
    {
        public UpdateViewModel ViewModel { get; }

        public UpdateView()
        {
            this.InitializeComponent();
            ViewModel = ServiceLocator.Current.GetService<UpdateViewModel>();
            // PropertyChanged event'ini burada bağlama - ViewModel hazır olduğunda
            this.Loaded += UpdateView_Loaded;
        }

        private async void UpdateView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                // ViewModel henüz initialize edilmediyse
                if (ViewModel.Settings == null)
                {
                    await ViewModel.InitializeAsync();
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ViewModel'deki TÜM değişikliklerde UI'yı güncelle
            DispatcherQueue.TryEnqueue(() =>
            {
                NotifyAllPropertiesChanged();
            });
        }

        // TÜM property'leri güncelle
        private void NotifyAllPropertiesChanged()
        {
            NotifyPropertyChanged(nameof(UpdateCardVisibility));
            NotifyPropertyChanged(nameof(UpdateDetailsVisibility));
            NotifyPropertyChanged(nameof(ChangelogButtonVisibility));
            NotifyPropertyChanged(nameof(ProgressVisibility));
            NotifyPropertyChanged(nameof(ErrorVisibility));
            NotifyPropertyChanged(nameof(StatusIconBrush));
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateView Initialize Error: {ex.Message}");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.Unsubscribe();
            }
        }
        public Visibility UpdateCardVisibility => ViewModel.ShouldShowUpdateCard() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UpdateDetailsVisibility => ViewModel.ShouldShowDetails() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ChangelogButtonVisibility => !string.IsNullOrEmpty(ViewModel.ChangelogUrl) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressVisibility => ViewModel.IsProgressVisible() ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ErrorVisibility => !string.IsNullOrEmpty(ViewModel.ErrorMessage) && ViewModel.CurrentState == UpdateState.Error ? Visibility.Visible : Visibility.Collapsed;

        public Brush StatusIconBrush => GetStatusIconBrush();
        private Brush GetStatusIconBrush()
        {
            var brushName = ViewModel.CurrentState switch
            {
                UpdateState.Idle or UpdateState.RestartRequired => "SystemFillColorSuccessBrush",
                UpdateState.Error => "SystemFillColorCriticalBrush",
                _ => "SystemControlForegroundAccentBrush",
            };
            return (Brush)Application.Current.Resources[brushName];
        }

        public ICommand OpenChangelogCommand => new AsyncRelayCommand(OpenChangelogAsync);
        private async Task OpenChangelogAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(ViewModel.ChangelogUrl))
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(ViewModel.ChangelogUrl));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Changelog açılamadı: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
