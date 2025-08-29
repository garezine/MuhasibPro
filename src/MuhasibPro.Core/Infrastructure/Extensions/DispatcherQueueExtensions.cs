namespace MuhasibPro.Core.Infrastructure.Extensions
{
    public static class DispatcherQueueExtensions
    {
        public static async Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            var result = dispatcher.TryEnqueue(() =>
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

            if (!result)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation"));
            }

            await tcs.Task;
        }
    }
}
