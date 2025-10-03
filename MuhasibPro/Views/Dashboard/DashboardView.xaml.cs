﻿using CommunityToolkit.Mvvm.DependencyInjection;
using MuhasibPro.ViewModels.Dashboard;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Dashboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            InitializeComponent();
        }
        public DashboardViewModel ViewModel { get; private set; } = Ioc.Default.GetService<DashboardViewModel>();
    }
}
