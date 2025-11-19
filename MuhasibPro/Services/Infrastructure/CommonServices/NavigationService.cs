using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using System.Collections.Concurrent;

namespace MuhasibPro.Services.Infrastructure.CommonServices;

public class NavigationService : INavigationService
{
    private static readonly ConcurrentDictionary<Type, Type> _viewModelMap = new();

    static NavigationService() { }

    public static int MainViewId
    {
        get
        {
            if (App.MainWindow?.AppWindow == null)
                return -1;
            return (int)App.MainWindow.AppWindow.Id.Value;
        }
    }

    // ViewModel-View mapping methods
    public static void Register<TViewModel, TView>() where TView : Page
    {
        if (!_viewModelMap.TryAdd(typeof(TViewModel), typeof(TView)))
        {
            throw new InvalidOperationException($"ViewModel already registered '{typeof(TViewModel).FullName}'");
        }
    }

    public static Type GetView<TViewModel>() { return GetView(typeof(TViewModel)); }

    public static Type GetView(Type viewModel)
    {
        if (_viewModelMap.TryGetValue(viewModel, out Type view))
        {
            return view;
        }
        throw new InvalidOperationException($"View not registered for ViewModel '{viewModel.FullName}'");
    }

    public static Type GetViewModel(Type view)
    {
        var type = _viewModelMap.Where(r => r.Value == view).Select(r => r.Key).FirstOrDefault();
        if (type == null)
        {
            throw new InvalidOperationException($"ViewModel not registered for View '{view.FullName}'");
        }
        return type;
    }

    public Frame Frame { get; private set; }

    public bool CanGoBack => Frame?.CanGoBack ?? false;

    // Navigation methods
    public void GoBack()
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
        }
    }

    public void Initialize(object frame) { Frame = frame as Frame; }

    public bool Navigate<TViewModel>(object parameter = null) { return Navigate(typeof(TViewModel), parameter); }

    public bool Navigate(Type viewModelType, object parameter = null)
    {
        if (Frame == null)
        {
            throw new InvalidOperationException("Navigation frame not initialized.");
        }
        return Frame.Navigate(GetView(viewModelType), parameter);
    }

    // Window creation methods - WindowManagerService kullanarak
    public async Task<int> CreateNewViewAsync<TViewModel>(object parameter = null, string customTitle = null)
    { return await CreateNewViewAsync(typeof(TViewModel), parameter, customTitle); }

    public async Task<int> CreateNewViewAsync(Type viewModelType, object parameter = null, string customTitle = null)
    { return await NavigationHelper.Instance.CreateWindowAsync(viewModelType, parameter, customTitle); }


    // Window closing methods - WindowManagerService kullanarak
    public async Task CloseViewAsync()
    {
        var currentWindow = CustomWindowHelper.CurrentWindow;
        if (currentWindow != null && currentWindow != App.MainWindow)
        {
            var windowId = CustomWindowHelper.GetWindowId(currentWindow);
            if (windowId > 0)
            {
                await NavigationHelper.Instance.CloseWindowAsync(windowId);

                // ✅ SMART ACTIVATION: Sadece gerekirse MainWindow'u aktif et
                var hasOtherChildWindows = CustomWindowHelper.GetAllWindows()
                    .Any(w => w != App.MainWindow && w != currentWindow);

                if (!hasOtherChildWindows)
                {
                    // Başka child window yok - MainWindow'u aktif et
                    CustomWindowHelper.TryActivateWindow(MainViewId);
                }
            }
        }
    }
}

