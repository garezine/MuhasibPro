using CommunityToolkit.Mvvm.Input;
using Muhasebe.Business.Common;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Controls;
using MuhasibPro.Infrastructure.Common;
using MuhasibPro.Infrastructure.ViewModels;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.Common;
public abstract partial class GenericListViewModel<TModel> : ViewModelBase where TModel : ObservableObject
{
    protected GenericListViewModel(ICommonServices commonServices) : base(commonServices)
    {
    }
    public override string Title => string.IsNullOrEmpty(Query) ? $" ({ItemsCount})" : $" ({ItemsCount} for \"{Query}\")";

    private IList<TModel> _items = null;
    public IList<TModel> Items
    {
        get => _items;
        set => Set(ref _items, value);
    }

    private int _itemsCount = 0;
    public int ItemsCount
    {
        get => _itemsCount;
        set => Set(ref _itemsCount, value);
    }

    private TModel _selectedItem = default;
    public TModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (Set(ref _selectedItem, value))
            {
                if (!IsMultipleSelection)
                {
                    MessageService.Send(this, "ItemSelected", _selectedItem);
                }
            }
        }
    }

    private string _query = null;
    public string Query
    {
        get => _query;
        set => Set(ref _query, value);
    }

    private ListToolbarMode _toolbarMode = ListToolbarMode.Default;
    public ListToolbarMode ToolbarMode
    {
        get => _toolbarMode;
        set => Set(ref _toolbarMode, value);
    }

    private bool _isMultipleSelection = false;
    public bool IsMultipleSelection
    {
        get => _isMultipleSelection;
        set => Set(ref _isMultipleSelection, value);
    }

    public List<TModel> SelectedItems
    {
        get; protected set;
    }
    public IndexRange[] SelectedIndexRanges
    {
        get; protected set;
    }

    public ICommand NewCommand => new RelayCommand(OnNew);

    public ICommand RefreshCommand => new RelayCommand(OnRefresh);

    public ICommand StartSelectionCommand => new RelayCommand(OnStartSelection);
    protected virtual void OnStartSelection()
    {
        StatusMessage("Seçimi başlat");
        StatusBar.ShowProgress("İşlem yapılıyor...");
        SelectedItem = null;
        SelectedItems = new List<TModel>();
        SelectedIndexRanges = null;
        IsMultipleSelection = true;
    }

    public ICommand CancelSelectionCommand => new RelayCommand(OnCancelSelection);
    protected virtual void OnCancelSelection()
    {
        StatusReady();
        SelectedItems = null;
        SelectedIndexRanges = null;
        IsMultipleSelection = false;
        SelectedItem = Items?.FirstOrDefault();
    }

    public ICommand SelectItemsCommand => new RelayCommand<IList<object>>(OnSelectItems);
    protected virtual void OnSelectItems(IList<object> items)
    {
        StatusReady();
        if (IsMultipleSelection)
        {
            SelectedItems.AddRange(items.Cast<TModel>());
            StatusMessage($"{SelectedItems.Count} öğe seçildi");
        }
    }

    public ICommand DeselectItemsCommand => new RelayCommand<IList<object>>(OnDeselectItems);
    protected virtual void OnDeselectItems(IList<object> items)
    {
        if (items?.Count > 0)
        {
            StatusReady();
        }
        if (IsMultipleSelection)
        {
            foreach (TModel item in items)
            {
                SelectedItems.Remove(item);
            }
            StatusMessage($"{SelectedItems.Count} öğe seçildi");
        }
    }

    public ICommand SelectRangesCommand => new RelayCommand<IndexRange[]>(OnSelectRanges);
    protected virtual void OnSelectRanges(IndexRange[] indexRanges)
    {
        SelectedIndexRanges = indexRanges;
        int count = SelectedIndexRanges?.Sum(r => r.Length) ?? 0;
        StatusMessage($"{count} öğe seçildi");
    }

    public ICommand DeleteSelectionCommand => new RelayCommand(OnDeleteSelection);

    protected abstract void OnNew();
    protected abstract void OnRefresh();
    protected abstract void OnDeleteSelection();

}

