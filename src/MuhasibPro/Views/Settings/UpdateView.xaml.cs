using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModel.Settings;

namespace MuhasibPro.Views.Settings
{
    public sealed partial class UpdateView : Page
    {
        public UpdateViewModel ViewModel { get; } = Ioc.Default.GetService<UpdateViewModel>();

        public UpdateView()
        {
            this.InitializeComponent();
            // PropertyChanged event'ini burada baðlama - ViewModel hazýr olduðunda
            this.Loaded += UpdateView_Loaded;
        }

        private void UpdateView_Loaded(object sender, RoutedEventArgs e)
        {
            // ViewModel tamamen yüklendiðinde event'i baðla
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // UI thread'de olduðumuzdan emin ol
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.CurrentState):
                        // Handle any specific state changes if needed
                        break;
                    case nameof(ViewModel.Settings):
                        // Settings deðiþtiðinde ComboBox'ý güncelle
                        SetCheckIntervalSelection();
                        break;
                }
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                await ViewModel.InitializeAsync();
                // Initialize tamamlandýktan sonra ComboBox'ý ayarla
                SetCheckIntervalSelection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateView Initialize Error: {ex.Message}");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Event handler'larý güvenli þekilde kaldýr
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.Unsubscribe();
            }
        }

        private void SetCheckIntervalSelection()
        {
            // UI thread'de olduðumuzdan emin ol ve null check'leri ekle
            DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel?.Settings != null && CheckIntervalComboBox != null)
                {
                    try
                    {
                        foreach (ComboBoxItem item in CheckIntervalComboBox.Items)
                        {
                            if (int.TryParse(item.Tag?.ToString(), out int tagValue) &&
                                tagValue == ViewModel.Settings.CheckIntervalHours)
                            {
                                CheckIntervalComboBox.SelectedItem = item;
                                return;
                            }
                        }

                        // Eðer eþleþen deðer bulunamazsa default olarak 24 saat'i seç
                        foreach (ComboBoxItem item in CheckIntervalComboBox.Items)
                        {
                            if (int.TryParse(item.Tag?.ToString(), out int tagValue) && tagValue == 24)
                            {
                                CheckIntervalComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SetCheckIntervalSelection Error: {ex.Message}");
                    }
                }
            });
        }

        private async void CheckIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ViewModel null check ve initialization tamamlanma kontrolü
            if (ViewModel?.Settings == null) return;

            if (CheckIntervalComboBox.SelectedItem is ComboBoxItem selected)
            {
                if (int.TryParse(selected.Tag?.ToString(), out int hours))
                {
                    try
                    {
                        await ViewModel.UpdateCheckInterval(hours);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateCheckInterval Error: {ex.Message}");
                    }
                }
            }
        }
    }
}