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
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void UnsubscribeFromEvents()
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle animation triggers
            switch (e.PropertyName)
            {
                case nameof(ViewModel.UpdateDetailsVisibility):
                    HandleUpdateDetailsVisibilityChanged();
                    break;
            }
        }

        private async void HandleUpdateDetailsVisibilityChanged()
        {
            if (ViewModel.UpdateDetailsVisibility == Visibility.Visible)
            {
                // Ensure the element is visible before animating
                UpdateDetailsGrid.Visibility = Visibility.Visible;
                await Task.Delay(50); // Small delay to ensure layout is updated
                ExpandStoryboard.Begin();
            }
            else
            {
                CollapseStoryboard.Completed += (s, e) =>
                {
                    UpdateDetailsGrid.Visibility = Visibility.Collapsed;
                };
                CollapseStoryboard.Begin();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.InitializeAsync();
            SetCheckIntervalSelection();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeFromEvents();
        }

        private void SetCheckIntervalSelection()
        {
            if (ViewModel.Settings != null)
            {
                foreach (ComboBoxItem item in CheckIntervalComboBox.Items)
                {
                    if (int.TryParse(item.Tag?.ToString(), out int tagValue) &&
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
                if (int.TryParse(selected.Tag?.ToString(), out int hours))
                {
                    await ViewModel.UpdateCheckInterval(hours);
                }
            }
        }
    }
}