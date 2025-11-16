using MuhasibPro.Extensions;
using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.ComponentModel;
using System.Windows.Input;

namespace MuhasibPro.Controls
{
    public class CommandConfig : DependencyObject, INotifyExpressionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DependencyExpressions _dependencyExpressions = new();
        private BaseConfig _parent;

        public CommandConfig()
        {
            _dependencyExpressions.Initialize(this);
        }

        // EKLE:
        internal void InitializeWithParent(BaseConfig parent)
        {
            _parent = parent;
            // RegisterDependencies yok çünkü cross-config dependency yok
        }
        #region ItemSecondaryActionInvokedCommand
        public ICommand ItemSecondaryActionInvokedCommand
        {
            get { return (ICommand)GetValue(ItemSecondaryActionInvokedCommandProperty); }
            set { SetValue(ItemSecondaryActionInvokedCommandProperty, value); }
        }

        public static readonly DependencyProperty ItemSecondaryActionInvokedCommandProperty = DependencyProperty.Register(nameof(ItemSecondaryActionInvokedCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region DefaultCommands
        public string DefaultCommands
        {
            get { return (string)GetValue(DefaultCommandsProperty); }
            set { SetValue(DefaultCommandsProperty, value); }
        }

        public static readonly DependencyProperty DefaultCommandsProperty = DependencyProperty.Register(nameof(DefaultCommands), typeof(string), typeof(CommandConfig), new PropertyMetadata("new,select,refresh,search"));
        #endregion

        #region RefreshCommand
        public ICommand RefreshCommand
        {
            get { return (ICommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region QuerySubmittedCommand
        public ICommand QuerySubmittedCommand
        {
            get { return (ICommand)GetValue(QuerySubmittedCommandProperty); }
            set { SetValue(QuerySubmittedCommandProperty, value); }
        }

        public static readonly DependencyProperty QuerySubmittedCommandProperty = DependencyProperty.Register(nameof(QuerySubmittedCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region NewCommand
        public ICommand NewCommand
        {
            get { return (ICommand)GetValue(NewCommandProperty); }
            set { SetValue(NewCommandProperty, value); }
        }

        public static readonly DependencyProperty NewCommandProperty = DependencyProperty.Register(nameof(NewCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region DeleteCommand
        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region StartSelectionCommand
        public ICommand StartSelectionCommand
        {
            get { return (ICommand)GetValue(StartSelectionCommandProperty); }
            set { SetValue(StartSelectionCommandProperty, value); }
        }

        public static readonly DependencyProperty StartSelectionCommandProperty = DependencyProperty.Register(nameof(StartSelectionCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region CancelSelectionCommand
        public ICommand CancelSelectionCommand
        {
            get { return (ICommand)GetValue(CancelSelectionCommandProperty); }
            set { SetValue(CancelSelectionCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelSelectionCommandProperty = DependencyProperty.Register(nameof(CancelSelectionCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region SelectItemsCommand
        public ICommand SelectItemsCommand
        {
            get { return (ICommand)GetValue(SelectItemsCommandProperty); }
            set { SetValue(SelectItemsCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemsCommandProperty = DependencyProperty.Register(nameof(SelectItemsCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region DeselectItemsCommand
        public ICommand DeselectItemsCommand
        {
            get { return (ICommand)GetValue(DeselectItemsCommandProperty); }
            set { SetValue(DeselectItemsCommandProperty, value); }
        }

        public static readonly DependencyProperty DeselectItemsCommandProperty = DependencyProperty.Register(nameof(DeselectItemsCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion

        #region SelectRangesCommand
        public ICommand SelectRangesCommand
        {
            get { return (ICommand)GetValue(SelectRangesCommandProperty); }
            set { SetValue(SelectRangesCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectRangesCommandProperty = DependencyProperty.Register(nameof(SelectRangesCommand), typeof(ICommand), typeof(CommandConfig), new PropertyMetadata(null));
        #endregion
        public void OnToolbarClick(object sender, ToolbarButtonClickEventArgs e)
        {
            switch (e.ClickedButton)
            {
                case ToolbarButton.New:
                    NewCommand?.TryExecute();
                    break;
                case ToolbarButton.Delete:
                    DeleteCommand?.TryExecute();
                    break;
                case ToolbarButton.Select:
                    StartSelectionCommand?.TryExecute();
                    break;
                case ToolbarButton.Refresh:
                    RefreshCommand?.TryExecute();
                    break;
                case ToolbarButton.Cancel:
                    CancelSelectionCommand?.TryExecute();
                    break;
            }
        }
        #region NotifyPropertyChanged
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}
