using Microsoft.UI.Xaml.Data;
using Muhasib.Business.Services.Contracts.LogServices;
using MuhasibPro.Extensions;
using System.Collections.Specialized;

namespace MuhasibPro.Common.VirtualCollection;

abstract public partial class VirtualCollection<T> : IItemsRangeInfo, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public readonly int RangeSize;

    private DispatcherTimer _timer = null;

    bool MustExploreDeepExceptions { get; set; }

    public VirtualCollection(ILogService logService, int rangeSize = 16, bool mustExploreDeepExceptions = false)
    {
        MustExploreDeepExceptions = mustExploreDeepExceptions;
        LogService = logService;

        RangeSize = rangeSize;
        Ranges = new Dictionary<int, IList<T>>();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(50);
        _timer.Tick += OnTimerTick;
    }

    public ILogService LogService { get; }

    public Dictionary<int, IList<T>> Ranges { get; }

    private bool _isBusy = false;
    private bool _cancel = false;

    private IReadOnlyList<ItemIndexRange> _trackedItems = null;

    public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
    { FetchRanges(trackedItems.Normalize().ToArray()); }

    private object _sync = new Object();

    private void OnTimerTick(object sender, object e) { FetchRanges(_trackedItems); }

    private async void FetchRanges(IReadOnlyList<ItemIndexRange> trackedItems)
    {
        _trackedItems = trackedItems;

        _timer.Stop();
        lock (_sync)
        {
            if (_isBusy)
            {
                _cancel = true;
                _timer.Start();
                return;
            }
            _cancel = false;
            _isBusy = true;
        }

        ClearUntrackedItems(trackedItems);
        await FetchRangesAsync(trackedItems);

        lock (_sync)
        {
            _isBusy = false;
        }
    }

    private void ClearUntrackedItems(IReadOnlyList<ItemIndexRange> trackedItems)
    {
        foreach (var rangeIndex in Ranges.Keys.ToArray())
        {
            bool isTracked = false;
            int index = rangeIndex * RangeSize;
            foreach (var trackedRange in trackedItems)
            {
                if (trackedRange.Intersects(index, (uint)RangeSize))
                {
                    isTracked = true;
                    break;
                }
            }
            if (!isTracked)
            {
                Ranges.Remove(rangeIndex);
            }
        }
    }

    private async Task FetchRangesAsync(IReadOnlyList<ItemIndexRange> trackedItems)
    {
        foreach (var trackedRange in trackedItems)
        {
            await FetchRange(trackedRange);
            if (_cancel)
                return;
        }
    }

    private async Task FetchRange(ItemIndexRange trackedRange)
    {
        int firstIndex = trackedRange.FirstIndex / RangeSize;
        int lastIndex = trackedRange.LastIndex / RangeSize;
        for (int index = firstIndex; index <= lastIndex; index++)
        {
            if (!Ranges.ContainsKey(index))
            {
                var items = await FetchDataAsync(index, RangeSize);
                if (items != null)
                {
                    Ranges[index] = items;
                    for (int n = 0; n < items.Count; n++)
                    {
                        int replaceIndex = Math.Min(index * RangeSize + n, Count - 1);
                        CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            items[n],
                            null,
                            replaceIndex));
                    }
                }
            }
        }
    }

    virtual public void Dispose()
    {
    }
    public abstract T DefaultItem { get; }

    public abstract Task<IList<T>> FetchDataAsync(int pageIndex, int pageSize);
}
