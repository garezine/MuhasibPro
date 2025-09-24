﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.ViewModels.Firmalar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Firmalar
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FirmalarView : Page
    {
        public FirmalarView()
        {
            InitializeComponent();
            ViewModel = Ioc.Default.GetService<FirmalarViewModel>();
            NavigationService = Ioc.Default.GetService<INavigationService>();
        }
        public FirmalarViewModel ViewModel { get; }
        public INavigationService NavigationService { get; }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Subscribe();
            await ViewModel.LoadAsync(e.Parameter as FirmaListArgs);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Unload();
            ViewModel.Unsubscribe();
        }
        private async void OpenInNewView(object sender, RoutedEventArgs e)
        {
            await NavigationService.CreateNewViewAsync<FirmalarViewModel>(ViewModel.FirmaList.CreateArgs(), customTitle: "Firmalar");
        }
        private async void OpenDetailsInNewView(object sender, RoutedEventArgs e)
        {
            ViewModel.FirmaDetails.CancelEdit();
            if (pivot.SelectedIndex == 0)
            {
                await NavigationService.CreateNewViewAsync<FirmaDetailsViewModel>(ViewModel.FirmaDetails.CreateArgs());
            }
            else
            {

            }
        }
        public int GetRowSpan(bool isMultipleSelection)
        {
            return isMultipleSelection ? 2 : 1;

        }

    }
}
