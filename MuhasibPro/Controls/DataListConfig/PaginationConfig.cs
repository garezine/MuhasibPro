using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.ComponentModel;
using System.Windows.Input;

namespace MuhasibPro.Controls
{
    public class PaginationConfig : DependencyObject, INotifyExpressionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DependencyExpressions _dependencyExpressions = new();
        private BaseConfig _parent;
        private ComboBox _pageSizeComboBox;
        private ComboBox _pageComboBox;
        public event EventHandler<double> AutoSizeModeRequested;
        public PaginationConfig()
        {
            _dependencyExpressions.Initialize(this);
            CurrentPage = 1;
            PageSize = 16;
            TotalItems = 0;
        }
        private CoreDataConfig CoreData => _parent.CoreData;
        // EKLE:
        internal void InitializeWithParent(BaseConfig parent)
        {
            if (_parent != null)
                return;
            _parent = parent;
            RegisterDependencies();
        }
        #region CurrentPage
        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PaginationConfig),
            new PropertyMetadata(1, OnCurrentPageChanged));
        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var config = (PaginationConfig)d;
            config._dependencyExpressions.UpdateDependencies(config, nameof(CurrentPage));
            config.UpdatePageComboBox();
        }
        #endregion

        #region PageSize
        public int PageSize { get => (int)GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(PaginationConfig),
            new PropertyMetadata(25, OnPageSizeChanged));
        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var config = (PaginationConfig)d;
            config._dependencyExpressions.UpdateDependencies(config, nameof(PageSize));
            config.UpdatePageComboBox();
        }
        #endregion

        #region TotalItems
        public int TotalItems
        {
            get => (int)GetValue(TotalItemsProperty);
            set => SetValue(TotalItemsProperty, value);
        }
        public static readonly DependencyProperty TotalItemsProperty =
            DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(PaginationConfig),
            new PropertyMetadata(0, OnTotalItemsChanged));
        private static void OnTotalItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var config = (PaginationConfig)d;
            config._dependencyExpressions.UpdateDependencies(config, nameof(TotalItems));
            config.UpdatePageComboBox();
        }
        #endregion
        #region Commands
        public ICommand PreviousPageCommand
        {
            get => (ICommand)GetValue(PreviousPageCommandProperty);
            set => SetValue(PreviousPageCommandProperty, value);
        }
        public static readonly DependencyProperty PreviousPageCommandProperty =
            DependencyProperty.Register(
            nameof(PreviousPageCommand),
            typeof(ICommand),
            typeof(PaginationConfig),
            new PropertyMetadata(null, OnPreviousPageCommandChanged));
        private static void OnPreviousPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { System.Diagnostics.Debug.WriteLine($"PaginationConfig PreviousPageCommand changed"); }
        public ICommand NextPageCommand
        {
            get => (ICommand)GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }
        public static readonly DependencyProperty NextPageCommandProperty =
            DependencyProperty.Register(
            nameof(NextPageCommand),
            typeof(ICommand),
            typeof(PaginationConfig),
            new PropertyMetadata(null, OnNextPageCommandChanged));
        private static void OnNextPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { System.Diagnostics.Debug.WriteLine($"PaginationConfig NextPageCommand changed"); }
        #endregion

        #region Calculated Properties
        public int TotalPages
        {
            get
            {
                if (TotalItems <= 0 || PageSize <= 0)
                    return 1;
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
        public bool CanGoToPreviousPage { get => CurrentPage > 1; }
        public bool CanGoToNextPage { get => CurrentPage < TotalPages; }
        public string PaginationInfo
        {
            get
            {
                if (TotalItems == 0)
                    return "Kayıt yok";
                var start = ((CurrentPage - 1) * PageSize) + 1;
                var end = Math.Min(CurrentPage * PageSize, TotalItems);
                return $"{start}-{end} / {TotalItems}";
            }
        }
        public List<int> PageNumbers
        {
            get
            {
                try
                {
                    var total = TotalPages;
                    if (total <= 0)
                        return new List<int> { 1 };
                    var numbers = Enumerable.Range(1, total).ToList();
                    return numbers ?? new List<int> { 1 }; // Null check
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PageNumbers calculation error: {ex.Message}");
                    return new List<int> { 1 };
                }
            }
        }
        public bool ShowPaginationControls { get => TotalItems > PageSize; }
        #endregion

        #region Otomatik Mod Özelliği
        public bool IsAutoSizeMode
        {
            get => (bool)GetValue(IsAutoSizeModeProperty);
            set => SetValue(IsAutoSizeModeProperty, value);
        }
        public static readonly DependencyProperty IsAutoSizeModeProperty =
            DependencyProperty.Register(
            nameof(IsAutoSizeMode),
            typeof(bool),
            typeof(PaginationConfig),
            new PropertyMetadata(false, OnIsAutoSizeModeChanged));
        private static void OnIsAutoSizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var config = (PaginationConfig)d;
            config._dependencyExpressions.UpdateDependencies(config, nameof(IsAutoSizeMode));
            if ((bool)e.NewValue)
            {
                // Otomatik mod açıldı - hemen hesapla
                System.Diagnostics.Debug.WriteLine("Otomatik mod aktif");
            }
        }
        #endregion

        #region ComboBox Management - YENİ EKLENEN
        public void InitializeComboBoxes(ComboBox pageSizeComboBox, ComboBox pageComboBox)
        {
            _pageSizeComboBox = pageSizeComboBox;
            _pageComboBox = pageComboBox;
            if (_pageSizeComboBox != null)
            {
                InitializePageSizeComboBox();
                _pageSizeComboBox.SelectionChanged += OnPageSizeComboBoxSelectionChanged;
                _pageSizeComboBox.Loaded += OnPageSizeComboBoxLoaded;
            }
            if (_pageComboBox != null)
            {
                InitializePageComboBox(); // ✅ Page combobox'ını doldur
                _pageComboBox.SelectionChanged += OnPageComboBoxSelectionChanged;
            }
        }
        private void InitializePageComboBox() { UpdatePageComboBox(); }
        private void InitializePageSizeComboBox()
        {
            if (_pageSizeComboBox == null)
                return;
            _pageSizeComboBox.Items.Clear();
            // Sabit değerler
            _pageSizeComboBox.Items.Add(new ComboBoxItem { Content = "16 kayıt", Tag = "16" });
            _pageSizeComboBox.Items.Add(new ComboBoxItem { Content = "25 kayıt", Tag = "25" });
            _pageSizeComboBox.Items.Add(new ComboBoxItem { Content = "50 kayıt", Tag = "50" });
            _pageSizeComboBox.Items.Add(new ComboBoxItem { Content = "100 kayıt", Tag = "100" });
            // Ayırıcı
            _pageSizeComboBox.Items
                .Add(
                    new ComboBoxItem
                    {
                        IsEnabled = false,
                        Content = new TextBlock { Text = "─────────", Opacity = 0.5 }
                    });
            // Otomatik mod
            _pageSizeComboBox.Items.Add(new ComboBoxItem { Content = "📏 Ekrana sığdır", Tag = "0" });
        }
        // ✅ Page combobox'ını güncelle
        private void UpdatePageComboBox()
        {
            if (_pageComboBox == null)
                return;
            try
            {
                var pages = PageNumbers;
                var currentPage = CurrentPage;
                // Event'i geçici olarak kapat
                _pageComboBox.SelectionChanged -= OnPageComboBoxSelectionChanged;
                _pageComboBox.ItemsSource = pages;
                _pageComboBox.SelectedItem = currentPage;
                // Event'i tekrar aç
                _pageComboBox.SelectionChanged += OnPageComboBoxSelectionChanged;
                System.Diagnostics.Debug
                    .WriteLine($"PageComboBox güncellendi: {pages.Count} sayfa, seçili: {currentPage}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePageComboBox error: {ex.Message}");
            }
        }
        private void OnPageSizeComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Varsayılan olarak "16 kayıt" seçilsin
            if (_pageSizeComboBox?.Items.Count > 0)
            {
                _pageSizeComboBox.SelectedIndex = 0;
            }
        }
        private void OnPageSizeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageSizeComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                HandlePageSizeSelection(selectedItem);
            }
        }
        private void OnPageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pageComboBox?.SelectedItem is int selectedPage && selectedPage != CurrentPage)
            {
                CurrentPage = selectedPage;
            }
        }
        private void HandlePageSizeSelection(ComboBoxItem selectedItem)
        {
            if (selectedItem.Tag?.ToString() == "0")
            {
                IsAutoSizeMode = true;
                System.Diagnostics.Debug.WriteLine("Otomatik mod seçildi");
                // ✅ DataList'ten doğru height değerini iste
                AutoSizeModeRequested?.Invoke(this, 0);
            }
            else if (int.TryParse(selectedItem.Tag?.ToString(), out int newPageSize) && newPageSize > 0)
            {
                IsAutoSizeMode = false;
                PageSize = newPageSize;
                CurrentPage = 1;
                System.Diagnostics.Debug.WriteLine($"Sabit PageSize: {newPageSize}");
            }
        }
        public void CalculateOptimalPageSize(double contentControlHeight)
        {
            try
            {
                if (contentControlHeight > 0 && IsAutoSizeMode)
                {
                    var rowHeight = 28;
                    var headerHeight = 40;
                    var footerHeight = 40;
                    var availableHeight = contentControlHeight - headerHeight - footerHeight;
                    var newPageSize = Math.Max(5, (int)Math.Floor(availableHeight / rowHeight));
                    if (newPageSize != PageSize)
                    {
                        PageSize = newPageSize;
                        System.Diagnostics.Debug.WriteLine($"Otomatik PageSize hesaplandı: {newPageSize}");
                    }
                }
            }
            catch
            {
                // Ignored 
            }
        }
        public void Cleanup()
        {
            if (_pageSizeComboBox != null)
            {
                _pageSizeComboBox.SelectionChanged -= OnPageSizeComboBoxSelectionChanged;
                _pageSizeComboBox.Loaded -= OnPageSizeComboBoxLoaded;
                _pageSizeComboBox = null;
            }
            if (_pageComboBox != null)
            {
                _pageComboBox.SelectionChanged -= OnPageComboBoxSelectionChanged;
                _pageComboBox = null;
            }
        }
        #endregion           

        #region Helper Methods
        private void RegisterDependencies()
        {
            // Calculated property'ler için dependency kayıtları
            _dependencyExpressions.Register(nameof(TotalPages), nameof(TotalItems), nameof(PageSize));
            _dependencyExpressions.Register(nameof(CanGoToPreviousPage), nameof(CurrentPage));
            _dependencyExpressions.Register(nameof(CanGoToNextPage), nameof(CurrentPage), nameof(TotalPages));
            _dependencyExpressions.Register(
                nameof(PaginationInfo),
                nameof(CurrentPage),
                nameof(PageSize),
                nameof(TotalItems));
            _dependencyExpressions.Register(nameof(PageNumbers), nameof(TotalPages));
            _dependencyExpressions.Register(nameof(ShowPaginationControls), nameof(TotalItems), nameof(PageSize));
        }
        #endregion

        #region NotifyPropertyChanged
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}