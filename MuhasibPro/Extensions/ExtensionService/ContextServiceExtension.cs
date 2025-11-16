using MuhasibPro.Helpers;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;

namespace MuhasibPro.Extensions.ExtensionService
{
    public static class ContextServiceExtension
    {
        /// <summary>
        /// ContextService'i initialize eder ve ViewContext'i kaydeder
        /// </summary>
        public static void InitializeWithContext(
            this IContextService contextService,
            object dispatcher,
            FrameworkElement viewElement)
        {
            var window = CustomWindowHelper.GetWindowForElement(viewElement);
            var contextId = CustomWindowHelper.GetWindowId(window);
            var mainViewId = CustomWindowHelper.GetWindowId(App.MainWindow);
            var isMainView = contextId == mainViewId;

            // ✅ TEK SATIR - sadece orijinal metodu çağır
            contextService.Initialize(dispatcher, contextId, isMainView);
        }
    }
}