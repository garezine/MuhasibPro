using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using System.Collections.Concurrent;

namespace MuhasibPro.Services.CommonServices;

public partial class NavigationService : INavigationService
{
    private static readonly ConcurrentDictionary<Type, Type> _viewModelMap = new();

    static NavigationService()
    {
        MainWindow = App.MainWindow;
    }

    public static Window MainWindow { get; private set; }

    // ViewModel-View mapping methods
    public static void Register<TViewModel, TView>() where TView : Page
    {
        if (!_viewModelMap.TryAdd(typeof(TViewModel), typeof(TView)))
        {
            throw new InvalidOperationException($"ViewModel already registered '{typeof(TViewModel).FullName}'");
        }
    }

    public static Type GetView<TViewModel>()
    {
        return GetView(typeof(TViewModel));
    }

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

    // Instance properties - WindowManagerService ile entegre

    public Window CurrentWindow => WindowHelper.CurrentWindow ?? MainWindow;

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

    public void Initialize(object frame)
    {
        Frame = frame as Frame;
    }

    public bool Navigate<TViewModel>(object parameter = null)
    {
        return Navigate(typeof(TViewModel), parameter);
    }

    public bool Navigate(Type viewModelType, object parameter = null)
    {
        if (Frame == null)
        {
            throw new InvalidOperationException("Navigation frame not initialized.");
        }
        return Frame.Navigate(GetView(viewModelType), parameter);
    }

    // Window creation methods - WindowManagerService kullanarak
    public async Task<Window> CreateNewViewAsync<TViewModel>(object parameter = null, string customTitle = null)
    {
        return await WindowManager.CreateWindowAsync<TViewModel>(parameter, customTitle);
    }

    public async Task<Window> CreateNewViewAsync(Type viewModelType, object parameter = null, string customTitle = null)
    {
        // Generic metodu çağır
        var method = typeof(WindowManager).GetMethod("CreateWindowAsync")
            .MakeGenericMethod(viewModelType);
        var task = (Task<Window>)method.Invoke(null, new object[] { parameter, customTitle });
        return await task;
    }


    // Window closing methods - WindowManagerService kullanarak
    public async Task CloseViewAsync()
    {
        var currentWindow = WindowHelper.CurrentWindow;
        if (currentWindow != null && currentWindow != MainWindow)
        {
            var windowId = WindowHelper.GetWindowId(currentWindow);
            if (windowId != null)
            {
                await WindowManager.CloseWindowAsync(windowId);
            }
        }
    }

}
