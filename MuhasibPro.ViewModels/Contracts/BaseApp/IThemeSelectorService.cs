using Microsoft.UI.Xaml;

namespace MuhasibPro.ViewModels.Contracts.BaseApp;

public interface IThemeSelectorService
{
    ElementTheme Theme
    {
        get;
    }

    Task InitializeAsync();

    Task SetThemeAsync(ElementTheme theme);

    Task SetRequestedThemeAsync();
}
