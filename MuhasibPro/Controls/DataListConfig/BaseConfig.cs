using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.ComponentModel;
using WinUI.TableView;

namespace MuhasibPro.Controls
{
    public class BaseConfig : DependencyObject, INotifyExpressionChanged
    {
        private CoreDataConfig _coreData;
        private SelectionConfig _selection;
        private CommandConfig _command;
        private PaginationConfig _pagination;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DependencyExpressions _dependencyExpressions = new();
        protected DependencyExpressions DependencyExpressions => _dependencyExpressions;
        public BaseConfig() { _dependencyExpressions.Initialize(this); }
        public CoreDataConfig CoreData
        {
            get => _coreData;
            set
            {
                _coreData = value;
                value?.InitializeWithParent(this);
            }
        }
        public SelectionConfig Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                value?.InitializeWithParent(this);
            }
        }
        public CommandConfig Command
        {
            get => _command;
            set
            {
                _command = value;
                value?.InitializeWithParent(this);
            }
        }
        public PaginationConfig Pagination
        {
            get => _pagination;
            set
            {
                _pagination = value;
                value?.InitializeWithParent(this);
            }
        }
        private TableView _getTableView = new TableView();
        public TableView GetTableView
        {
            get => _getTableView;
            set => _getTableView = value;
        }
        public void AttachTableView(TableView tableView)
        {
            System.Diagnostics.Debug.WriteLine("=== AttachTableView ===");
            GetTableView = tableView;

            if (tableView != null && CoreData != null)
            {
                System.Diagnostics.Debug.WriteLine("Clearing existing filters...");
                tableView.FilterDescriptions.Clear();

                System.Diagnostics.Debug.WriteLine("Adding new FilterDescription...");
                tableView.FilterDescriptions.Add(
                    new FilterDescription(string.Empty, CoreData.FilterItem)
                );

                System.Diagnostics.Debug.WriteLine($"Filter added. Total filters: {tableView.FilterDescriptions.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERROR - tableView: {tableView != null}, CoreData: {CoreData != null}");
            }
        }
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
