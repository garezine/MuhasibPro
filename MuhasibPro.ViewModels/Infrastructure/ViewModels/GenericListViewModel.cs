using Muhasib.Business.Models.Common;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Domain.Enum;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Extensions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.Infrastructure.ViewModels;

public abstract partial class GenericListViewModel<TModel> : ViewModelBase where TModel : ObservableObject
{
    protected GenericListViewModel(ICommonServices commonServices) : base(commonServices)
    {
    }

    public override string Title => string.IsNullOrEmpty(Query)
        ? $" ({ItemsCount})"
        : $" ({ItemsCount} for \"{Query}\")";

    private IList<TModel> _items = null;

    public IList<TModel> Items { get => _items; set => Set(ref _items, value); }

    private int _itemsCount = 0;

    public int ItemsCount
    {
        get => _itemsCount;
        set
        {
            if(Set(ref _itemsCount, Math.Max(0, value))) // Negatif değerleri önle
            {
                UpdatePaginationInfo();
            }
        }
    }

    private ObservableRangeCollection<TModel> _itemsSource = new ObservableRangeCollection<TModel>();

    public ObservableRangeCollection<TModel> ItemsSource { get => _itemsSource; set => Set(ref _itemsSource, value); }

    private TModel _selectedItem = default;

    public TModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if(Set(ref _selectedItem, value))
            {
                // SelectedItem değiştiğinde IsItemSelected'ı güncelle
                NotifyPropertyChanged(nameof(IsItemSelected));
                if(!IsMultipleSelection)
                {
                    MessageService.Send(this, "ItemSelected", _selectedItem);
                }
            }
        }
    }

    private string _query = null;

    public string Query { get => _query; set => Set(ref _query, value); }

    private ListToolbarMode _toolbarMode = ListToolbarMode.Default;

    public ListToolbarMode ToolbarMode { get => _toolbarMode; set => Set(ref _toolbarMode, value); }

    private bool _isMultipleSelection = false;

    public bool IsMultipleSelection { get => _isMultipleSelection; set => Set(ref _isMultipleSelection, value); }

    public List<TModel> SelectedItems { get; protected set; }

    public IndexRange[] SelectedIndexRanges { get; protected set; }

    public ICommand NewCommand => new RelayCommand(OnNew);

    public ICommand RefreshCommand => new RelayCommand(OnRefresh);

    public ICommand StartSelectionCommand => new RelayCommand(OnStartSelection);

    protected virtual void OnStartSelection()
    {
        StatusActionMessage("Çoklu seçim modu", StatusMessageType.MultipleSelect, autoHide: -1);
        SelectedItem = null;
        SelectedItems = new List<TModel>();
        SelectedIndexRanges = null;
        IsMultipleSelection = true;
    }

    public bool IsItemSelected => SelectedItem != null && !IsMultipleSelection;

    // SelectedItems değiştiğinde de güncelle (çoklu seçim için)
    protected virtual void OnSelectedItemsChanged() { NotifyPropertyChanged(nameof(IsItemSelected)); }
    protected virtual void OnSelectedIndexRangesChanged() { NotifyPropertyChanged(nameof(IsItemSelected)); }

    public ICommand CancelSelectionCommand => new RelayCommand(OnCancelSelection);

    protected virtual void OnCancelSelection()
    {
        StatusReady();
        SelectedItems = null;
        SelectedIndexRanges = null;
        IsMultipleSelection = false;
        SelectedItem = Items?.FirstOrDefault();
        NotifyPropertyChanged(nameof(IsItemSelected));
    }

    public ICommand SelectItemsCommand => new RelayCommand<IList<object>>(OnSelectItems);

    protected virtual void OnSelectItems(IList<object> items)
    {
        StatusReady();
        if(IsMultipleSelection)
        {
            // NULL KONTROLÜ EKLE
            SelectedItems ??= new List<TModel>();
            SelectedItems.AddRange(items.Cast<TModel>());
            OnSelectedItemsChanged();
            StatusActionMessage($"{SelectedItems.Count} öğe seçildi",StatusMessageType.MultipleSelect, autoHide: -1);
        }
    }

    public ICommand DeselectItemsCommand => new RelayCommand<IList<object>>(OnDeselectItems);

    protected virtual void OnDeselectItems(IList<object> items)
    {
        if(items?.Count > 0)
        {
            StatusReady();
        }
        if(IsMultipleSelection)
        {
            foreach(TModel item in items)
            {
                SelectedItems.Remove(item);
            }
            OnSelectedItemsChanged(); // Yeni ekle
            StatusActionMessage($"{SelectedItems.Count} öğe seçildi", StatusMessageType.MultipleSelect, autoHide: -1);
        }
    }

    public ICommand SelectRangesCommand => new RelayCommand<IndexRange[]>(OnSelectRanges);

    protected virtual void OnSelectRanges(IndexRange[] indexRanges)
    {
        SelectedIndexRanges = indexRanges;
        OnSelectedIndexRangesChanged(); // Yeni ekle
        int count = SelectedIndexRanges?.Sum(r => r.Length) ?? 0;
        StatusActionMessage($"{count} öğe seçildi", StatusMessageType.MultipleSelect, autoHide: -1);
    }

    public ICommand DeleteSelectionCommand => new RelayCommand(OnDeleteSelection);

    #region Sayfalama 
    private int _currentPage = 1;
    private int _pageSize = 16;

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            // Geçerli sayfa aralığını kontrol et
            var validPage = Math.Max(1, Math.Min(value, TotalPages));
            if(Set(ref _currentPage, validPage))
            {
                System.Diagnostics.Debug.WriteLine($"ViewModel CurrentPage set to: {_currentPage}");
                SelectedItem = null;
                LoadPageAsync();
                UpdatePaginationInfo();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if(Set(ref _pageSize, value))
            {
                // PageSize değişince ilk sayfaya dön
                CurrentPage = 1;
                LoadPageAsync();
            }
        }
    }

    public int TotalPages => (int)Math.Ceiling((double)ItemsCount / PageSize);

    public List<int> PageNumbers
    {
        get
        {
            try
            {
                var total = TotalPages;
                if(total <= 0)
                    return new List<int> { 1 }; // Null yerine en az [1]
                return Enumerable.Range(1, total).ToList();
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ViewModel PageNumbers error: {ex.Message}");
                return new List<int> { 1 }; // Fallback
            }
        }
    }

    public bool CanGoToPreviousPage => CurrentPage > 1;

    public bool CanGoToNextPage => CurrentPage < TotalPages;

    public ICommand PreviousPageCommand => new RelayCommand(
        () =>
        {
            if(CanGoToPreviousPage)
                CurrentPage--;
        });

    public ICommand NextPageCommand => new RelayCommand(
        () =>
        {
            if(CanGoToNextPage)
                CurrentPage++;
        });

    public bool ShowPagination => ItemsCount > PageSize;

    public void UpdatePaginationInfo()
    {
        NotifyPropertyChanged(nameof(CanGoToPreviousPage));
        NotifyPropertyChanged(nameof(CanGoToNextPage));
        NotifyPropertyChanged(nameof(ShowPagination));  // YENİ
        NotifyPropertyChanged(nameof(TotalPages));
        NotifyPropertyChanged(nameof(PageNumbers));
        ContextService.RunAsync(
            () =>
            {
                NotifyPropertyChanged(nameof(CurrentPage));
            });
    }
    #endregion
    // GenericListViewModel'e ekle:
    protected virtual async void LoadPageAsync()
    {
        try
        {
            // Sayfa validasyonu
            if(CurrentPage < 1)
                CurrentPage = 1;
            if(TotalPages > 0 && CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                return;
            }
            await ExecuteWithProgressAsync(
                action: async () => await LoadDataAsync(),
                progressMessage: $"{CurrentPage}. sayfa yükleniyor",
                successMessage: $"{CurrentPage}. sayfa yüklendi",
                errorMessage: $"Sayfa {CurrentPage} yüklenirken hata oluştu"
         );
            UpdatePaginationInfo();
        }
       
        catch (Exception ex)
        {
            StatusError($"Sayfa {CurrentPage} yüklenirken beklenmeyen hata");
            LogSistemException("LoadPageAsync","Veriler yüklenirken hata oluştu" ,ex);
        }
    }

    public async Task RemoveFromUIWithAnimationAsync(IEnumerable<IndexRange> ranges)
    {
        if (ItemsSource == null)
            return;

        // Tüm silinecek indexleri topla ve tersten sırala
        var allIndexesToRemove = new List<int>();

        foreach (var range in ranges.ToList())
        {
            if (range.Index >= 0 && range.Index < ItemsSource.Count && range.Length > 0)
            {
                int maxLength = Math.Min(range.Length, ItemsSource.Count - range.Index);
                for (int i = 0; i < maxLength; i++)
                {
                    allIndexesToRemove.Add(range.Index + i);
                }
            }
        }

        // Tekrar edenleri kaldır ve tersten sırala (animasyon için önemli)
        var sortedIndexes = allIndexesToRemove.Distinct().OrderByDescending(x => x).ToList();

        // Her silme işlemi arasında delay ekle (animasyon için)
        foreach (var index in sortedIndexes)
        {
            if (index < ItemsSource.Count)
            {
                ItemsSource.RemoveAt(index);

                // Animasyonun gözükmesi için yeterli delay
                await Task.Delay(150); // 150ms - animasyon süresine göre ayarlayın
            }
        }
    }
    public async Task RemoveItemsFromUIWithAnimationAsync(IEnumerable<TModel> items)
    {
        if (ItemsSource == null)
            return;

        var itemsToRemove = items.ToList();

        foreach (var item in itemsToRemove)
        {
            if (ItemsSource.Contains(item))
            {
                ItemsSource.Remove(item);

                // Animasyonun gözükmesi için yeterli delay
                await Task.Delay(150); // 150ms - animasyon süresine göre ayarlayın
            }
        }
    }
    protected abstract Task LoadDataAsync(); // ← PARAMETRESİZ

    protected abstract void OnNew();

    protected abstract void OnRefresh();

    protected abstract void OnDeleteSelection();
}

