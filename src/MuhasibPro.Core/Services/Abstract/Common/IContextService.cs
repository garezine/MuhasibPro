using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Security.Cryptography;

namespace MuhasibPro.Core.Services.Abstract.Common;

public interface IContextService
{
    int MainViewID { get; }
    int ContextId { get; }
    bool IsMainView { get; }    
    Task RunAsync(Action action);
    void InitializeWithViewType(object dispatcher, ViewType viewType, FrameworkElement viewElement, string viewName = null);


}
public class ViewContext
{
    public int ContextId { get; set; }
    public ViewType ViewType { get; set; }
    public bool IsMainView { get; set; }
    public DispatcherQueue DispatcherQueue { get; set; }
    public FrameworkElement ViewElement { get; set; }
    public string ViewName { get; set; }
}
public enum ViewType
{
    Login,          // Giriş ekranı
    MainShell,      // NavigationView ana container
    Shell,          // Sayfalar için content container
    ContentPage     // Tekil sayfalar
}
