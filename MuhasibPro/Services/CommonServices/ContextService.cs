using Microsoft.UI.Dispatching;
using MuhasibPro.ViewModels.Contracts.CommonServices;

namespace MuhasibPro.Services.CommonServices
{
    public class ContextService : IContextService
    {
        private static int _mainViewID = -1;
        private static readonly object _lock = new object();
        private static readonly Dictionary<int, ViewContext> _viewContexts = new();

        private DispatcherQueue _dispatcherQueue = null;

        public int MainViewID => _mainViewID;
        public int ContextId { get; private set; }
        public bool IsMainView { get; private set; }
        public ViewType CurrentViewType { get; private set; }

        public void Initialize(object dispatcher, int contextID, bool isMainView)
        {
            _dispatcherQueue = dispatcher as DispatcherQueue;
            ContextId = contextID;
            IsMainView = isMainView;

            if (IsMainView)
            {
                lock (_lock)
                {
                    if (_mainViewID == -1)
                    {
                        _mainViewID = ContextId;
                    }
                }
            }
        }

        // View tipine göre context initialize etmek için
        public void InitializeWithViewType(object dispatcher, ViewType viewType, FrameworkElement viewElement, string viewName = null)
        {
            var contextId = viewElement.GetHashCode();
            var isMainView = viewType == ViewType.MainShell; // MainShell'i ana view olarak kabul ediyoruz

            Initialize(dispatcher, contextId, isMainView);
            CurrentViewType = viewType;

            RegisterViewContext(contextId, viewType, isMainView, dispatcher as DispatcherQueue, viewElement, viewName);
        }



        private static void RegisterViewContext(int contextId, ViewType viewType, bool isMainView,
            DispatcherQueue dispatcherQueue, FrameworkElement viewElement, string viewName)
        {
            lock (_lock)
            {
                _viewContexts[contextId] = new ViewContext
                {
                    ContextId = contextId,
                    ViewType = viewType,
                    IsMainView = isMainView,
                    DispatcherQueue = dispatcherQueue,
                    ViewElement = viewElement,
                    ViewName = viewName ?? viewType.ToString()
                };
            }
        }

        public static ViewContext GetViewContext(int contextId)
        {
            lock (_lock)
            {
                return _viewContexts.TryGetValue(contextId, out var context) ? context : null;
            }
        }

        public static ViewContext GetViewContextByType(ViewType viewType)
        {
            lock (_lock)
            {
                foreach (var context in _viewContexts.Values)
                {
                    if (context.ViewType == viewType)
                        return context;
                }
                return null;
            }
        }

        public static IEnumerable<ViewContext> GetAllViewContexts()
        {
            lock (_lock)
            {
                return new List<ViewContext>(_viewContexts.Values);
            }
        }

        public async Task RunAsync(Action action)
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                action();
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        action();
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
                await tcs.Task;
            }
        }

        // Belirli bir view tipinde action çalıştırmak için
        public static async Task RunOnViewAsync(ViewType viewType, Action action)
        {
            var context = GetViewContextByType(viewType);
            if (context?.DispatcherQueue != null)
            {
                if (context.DispatcherQueue.HasThreadAccess)
                {
                    action();
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>();
                    context.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            action();
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    });
                    await tcs.Task;
                }
            }
        }

        // Cleanup metodu
        public static void UnregisterViewContext(int contextId)
        {
            lock (_lock)
            {
                _viewContexts.Remove(contextId);
            }
        }
    }
}
