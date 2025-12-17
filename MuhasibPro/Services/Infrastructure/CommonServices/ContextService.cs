using Microsoft.UI.Dispatching;
using Muhasib.Business.Services.Contracts.CommonServices;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public class ContextService : IContextService
    {
        private static int _mainViewID = -1;
        private DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        public int MainViewID => _mainViewID;
        public int ContextId { get; private set; }
        public bool IsMainView { get; private set; }
        public void Initialize(object dispatcher, int contextID, bool isMainView)
        {
            _dispatcherQueue = dispatcher as DispatcherQueue;
            ContextId = contextID;
            IsMainView = isMainView;
            if (IsMainView)
            {
                _mainViewID = contextID;
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
                _dispatcherQueue.TryEnqueue(
                    () =>
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
}


