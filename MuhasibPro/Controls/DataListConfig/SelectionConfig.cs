using MuhasibPro.Extensions;
using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.ComponentModel;

namespace MuhasibPro.Controls
{
    public class SelectionConfig : DependencyObject, INotifyExpressionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DependencyExpressions _dependencyExpressions = new();
        private BaseConfig _parent;
        public SelectionConfig()
        {
            _dependencyExpressions.Initialize(this);
        }

        // EKLE:
        internal void InitializeWithParent(BaseConfig parent)
        {
            if (_parent != null) return;
            _parent = parent;
            RegisterDependencies();
        }

        private CoreDataConfig CoreData => _parent.CoreData;
        private CommandConfig Command => _parent.Command;
        #region IsMultipleSelection*
        public bool IsMultipleSelection
        {
            get { return (bool)GetValue(IsMultipleSelectionProperty); }
            set { SetValue(IsMultipleSelectionProperty, value); }
        }

        private static void IsMultipleSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SelectionConfig)d;
            control._dependencyExpressions.UpdateDependencies(control, nameof(IsMultipleSelection));
        }

        public static readonly DependencyProperty IsMultipleSelectionProperty = DependencyProperty.Register(nameof(IsMultipleSelection), typeof(bool), typeof(SelectionConfig), new PropertyMetadata(null, IsMultipleSelectionChanged));
        #endregion

        public ListViewSelectionMode SelectionMode => IsMultipleSelection ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;

        public ListToolbarMode ToolbarMode => IsMultipleSelection ? (CoreData.SelectedItemsCount > 0 ? ListToolbarMode.CancelDelete : ListToolbarMode.Cancel) : ListToolbarMode.Default;

        public bool IsSingleSelection => !IsMultipleSelection;


        public string SelectionInfo =>
           !IsMultipleSelection || CoreData.SelectedItemsCount == 0 ? string.Empty : $"{CoreData.SelectedItemsCount} öğe seçildi.";


        public void OnDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (!IsMultipleSelection)
            {
                Command.ItemSecondaryActionInvokedCommand?.TryExecute(_parent.GetTableView.SelectedItem);
            }
        }
        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsMultipleSelection) return;

            var tableView = _parent?.GetTableView;
            if (tableView == null) return;
            if (IsMultipleSelection)
            {
                if (CoreData.SelectedItem != null)
                {
                    CoreData.SelectedItemsCount = _parent.GetTableView.SelectedItems.Count;
                }
                else if (_parent.GetTableView.SelectedRanges != null)
                {
                    var ranges = _parent.GetTableView.SelectedRanges;
                    CoreData.SelectedItemsCount = ranges.IndexCount();
                    Command.SelectRangesCommand?.TryExecute(ranges.GetIndexRanges().ToArray());
                }

                if (e.AddedItems != null)
                {
                    Command.SelectItemsCommand?.TryExecute(e.AddedItems);
                }
                if (e.RemovedItems != null)
                {
                    Command.DeselectItemsCommand?.TryExecute(e.RemovedItems);
                }

            }

        }
        private void RegisterDependencies()
        {
            _dependencyExpressions.Register(nameof(SelectionMode), nameof(IsMultipleSelection));
            _dependencyExpressions.Register(nameof(IsSingleSelection), nameof(IsMultipleSelection));
            // ToolbarMode ve SelectionInfo, CoreData.SelectedItemsCount'a bağımlı
            // Manuel subscription gerekli:
            if (CoreData != null)
            {
                _dependencyExpressions.Register(nameof(ToolbarMode), nameof(IsMultipleSelection), nameof(CoreData.SelectedItemsCount));
                CoreData.PropertyChanged += OnCoreDataPropertyChanged;
            }
        }

        private void OnCoreDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CoreData.SelectedItemsCount))
            {
                NotifyPropertyChanged(nameof(ToolbarMode));
                NotifyPropertyChanged(nameof(SelectionInfo));
            }
        }

        #region NotifyPropertyChanged
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

    }
}
