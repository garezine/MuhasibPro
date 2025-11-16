namespace MuhasibPro.Extensions.ExtensionService
{
    public static class DispatcherQueueExtensions
    {
        public static async Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.TryEnqueue(() =>
            {
                try { action(); tcs.SetResult(true); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            await tcs.Task;
        }
    }
}
