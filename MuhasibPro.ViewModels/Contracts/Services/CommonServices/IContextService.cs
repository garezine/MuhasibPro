namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices;

public interface IContextService
{
    int MainViewID { get; }
    int ContextId { get; }
    bool IsMainView { get; }    

    Task RunAsync(Action action);
    void Initialize(object dispatcher, int contextID, bool isMainView);

}

public enum ViewType
{
    Login,          // Giriş ekranı
    MainShell,      // NavigationView ana container
    Shell,          // Sayfalar için content container
    ContentPage     // Tekil sayfalar
}
