using Microsoft.UI.Xaml.Data;
using MuhasibPro.Extensions;
using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MuhasibPro.Controls
{
    public class CoreDataConfig : DependencyObject, INotifyExpressionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DependencyExpressions _dependencyExpressions = new();
        private BaseConfig _parent;
        private CancellationTokenSource? _filterToken;
        public CoreDataConfig() { _dependencyExpressions.Initialize(this); }
        // EKLE:
        internal void InitializeWithParent(BaseConfig parent)
        {
            if (_parent != null)
                return;
            _parent = parent;
            RegisterDependencies();
        }
        private SelectionConfig Selection => _parent.Selection;
        private CommandConfig Command => _parent.Command;
        #region ItemsSource*
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CoreDataConfig;
            control.UpdateItemsSource(e.NewValue, e.OldValue);
            control._dependencyExpressions.UpdateDependencies(control, nameof(ItemsSource));
        }

        public void UpdateItemsSource(object newValue, object oldValue)
        {
            if (oldValue is INotifyCollectionChanged oldSource)
            {
                oldSource.CollectionChanged -= OnCollectionChanged;
            }
            if (newValue is INotifyCollectionChanged newSource)
            {
                newSource.CollectionChanged += OnCollectionChanged;
            }
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Selection == null)
                return;
            if (!Selection.IsMultipleSelection)
            {
                if (ItemsSource is IList list)
                {
                    if (e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        if (ItemsSource is ISelectionInfo selectionInfo)
                        {
                            if (selectionInfo.IsSelected(e.NewStartingIndex))
                            {
                                SelectedItem = list[e.NewStartingIndex];
                                System.Diagnostics.Debug.WriteLine("SelectedItem {0}", SelectedItem);
                            }
                        }
                    }
                }
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(CoreDataConfig),
            new PropertyMetadata(null, ItemsSourceChanged));
        #endregion

        #region SelectedItem
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CoreDataConfig),
            new PropertyMetadata(null));
        #endregion

        #region SelectedItemsCount*
        public int SelectedItemsCount
        {
            get { return (int)GetValue(SelectedItemsCountProperty); }
            set { SetValue(SelectedItemsCountProperty, value); }
        }
        private static void SelectedItemsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CoreDataConfig;
            control.NotifyPropertyChanged(nameof(SelectedItemsCount));
            control._dependencyExpressions.UpdateDependencies(control, nameof(SelectedItemsCount));
        }
        public static readonly DependencyProperty SelectedItemsCountProperty = DependencyProperty.Register(
            nameof(SelectedItemsCount),
            typeof(int),
            typeof(CoreDataConfig),
            new PropertyMetadata(null, SelectedItemsCountChanged));
        #endregion

        #region ItemsCount
        public int ItemsCount
        {
            get { return (int)GetValue(ItemsCountProperty); }
            set { SetValue(ItemsCountProperty, value); }
        }
        public static readonly DependencyProperty ItemsCountProperty = DependencyProperty.Register(
            nameof(ItemsCount),
            typeof(int),
            typeof(CoreDataConfig),
            new PropertyMetadata(0));
        #endregion

        #region Query - Query Helper Metod
        public string Query { get { return (string)GetValue(QueryProperty); } set { SetValue(QueryProperty, value); } }
        private static void OnQueryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CoreDataConfig;
            System.Diagnostics.Debug.WriteLine($"Query changed: '{e.NewValue}'");
            control?.OnQueryChangedAsync();
        }
        public static readonly DependencyProperty QueryProperty =
            DependencyProperty.Register(
            nameof(Query),
            typeof(string),
            typeof(CoreDataConfig),
            new PropertyMetadata(null, OnQueryChanged));
        private async void OnQueryChangedAsync()
        {
            if (_filterToken is not null)
            {
                _filterToken.Cancel();
            }
            _filterToken = new CancellationTokenSource();
            await RefreshFilterAsync(_filterToken.Token);
        }
        private async Task RefreshFilterAsync(CancellationToken token)
        {
            try
            {
                // Debounce: 300ms bekle
                await Task.Delay(200, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            _filterToken = null;
            System.Diagnostics.Debug.WriteLine("Refreshing filter...");
            // TableView'i filtrele
            _parent?.GetTableView?.RefreshFilter();
        }
        // Filter metodu - TableView için
        public bool FilterItem(object? item)
        {
            if (string.IsNullOrWhiteSpace(Query))
                return true;
            if (item == null)
                return false;
            try
            {
                var properties = item.GetType().GetProperties().Where(p => p.CanRead);
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    if (value == null)
                        continue;
                    // Her property'nin string değerini kontrol et
                    if (value.ToString()?.Contains(Query, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        #endregion

        #region NewLabel
        public string NewLabel
        {
            get { return (string)GetValue(NewLabelProperty); }
            set { SetValue(NewLabelProperty, value); }
        }
        public static readonly DependencyProperty NewLabelProperty = DependencyProperty.Register(
            nameof(NewLabel),
            typeof(string),
            typeof(CoreDataConfig),
            new PropertyMetadata("New"));
        #endregion
        public bool IsDataAvailable => (ItemsSource?.Cast<object>().Any() ?? false);
        public bool IsDataUnavailable => !IsDataAvailable;
        public string DataUnavailableMessage => ItemsSource == null ? "Yükleniyor..." : "Veri bulunamadı..!";
        public void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        { Command?.QuerySubmittedCommand?.TryExecute(args.QueryText); }
        public object ListContent { get => GetValue(ListContentProperty); set => SetValue(ListContentProperty, value); }
        public static readonly DependencyProperty ListContentProperty =
            DependencyProperty.Register(
            nameof(ListContent),
            typeof(object),
            typeof(CoreDataConfig),
            new PropertyMetadata(null, OnListContentChanged));
        private static void OnListContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CoreDataConfig;
            control._dependencyExpressions.UpdateDependencies(control, nameof(ListContent));
            //((BaseConfig)d).NotifyPropertyChanged(nameof(ListContent));
        }
        private void RegisterDependencies()
        {
            _dependencyExpressions.Register(nameof(IsDataAvailable), nameof(ItemsSource));
            _dependencyExpressions.Register(nameof(IsDataUnavailable), nameof(IsDataAvailable));
            _dependencyExpressions.Register(nameof(DataUnavailableMessage), nameof(ItemsSource));
        }
        #region NotifyPropertyChanged
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}
