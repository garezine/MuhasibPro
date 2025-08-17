using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.ViewModel.Settings;
using System.Threading.Tasks;

namespace MuhasibPro.Views.Settings
{
    public sealed partial class UpdateView : Page
    {
        public UpdateViewModel ViewModel { get; } = Ioc.Default.GetService<UpdateViewModel>();

        public UpdateView()
        {
            this.InitializeComponent();

            // Dialog event'lerini baðla
            ViewModel.ErrorDialogRequested += ShowErrorDialog;
            ViewModel.InfoDialogRequested += ShowInfoDialog;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {            
            SetCheckIntervalSelection();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private void SetCheckIntervalSelection()
        {
            if (ViewModel.Settings != null)
            {
                foreach (ComboBoxItem item in CheckIntervalComboBox.Items)
                {
                    if (int.TryParse(item.Tag.ToString(), out int tagValue) &&
                        tagValue == ViewModel.Settings.CheckIntervalHours)
                    {
                        CheckIntervalComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private async void CheckIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CheckIntervalComboBox.SelectedItem is ComboBoxItem selected)
            {
                if (int.TryParse(selected.Tag.ToString(), out int hours))
                {
                    await ViewModel.UpdateCheckInterval(hours);
                }
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Hata",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowInfoDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Bilgi",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}