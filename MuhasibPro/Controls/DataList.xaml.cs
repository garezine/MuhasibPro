using MuhasibPro.Tools.DependencyExpressions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using System.ComponentModel;
using WinUI.TableView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Controls
{
    public sealed partial class DataList : UserControl, INotifyExpressionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherTimer _resizeTimer;
        static private readonly DependencyExpressions DependencyExpressions = new DependencyExpressions();
        public DataList()
        {
            InitializeComponent();
            DependencyExpressions.Initialize(this);
            if (contentControl != null)
            {
                contentControl.SizeChanged += OnContentControlSizeChanged;
                contentControl.RegisterPropertyChangedCallback(
                    ContentControl.ContentProperty,
                    OnContentControlContentChanged);
            }
        }
        public BaseConfig ConfigControl
        {
            get { return (BaseConfig)GetValue(ConfigControlProperty); }
            set { SetValue(ConfigControlProperty, value); }
        }
        public static readonly DependencyProperty ConfigControlProperty =
            DependencyProperty.Register(
            nameof(ConfigControl),
            typeof(BaseConfig),
            typeof(DataList),
            new PropertyMetadata(null, OnConfigControlChanged)); // ✅ Callback ekle
        private static void OnConfigControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataList = (DataList)d;
            var newConfig = e.NewValue as BaseConfig;
            var oldConfig = e.OldValue as BaseConfig;
            oldConfig?.Pagination?.Cleanup();
            if (newConfig?.Pagination != null && dataList.pageSizeComboBox != null && dataList.pageComboBox != null)
            {
                newConfig.Pagination.InitializeComboBoxes(dataList.pageSizeComboBox, dataList.pageComboBox);
                // ✅ Otomatik mod isteği geldiğinde doğru height ile hesapla
                newConfig.Pagination.AutoSizeModeRequested += (s, height) =>
                {
                    dataList.CalculateOptimalPageSize();
                };
            }
        }
        private void CalculateOptimalPageSize(object listContent = null)
        {
            try
            {
                if (contentControl?.ActualHeight > 0 && ConfigControl?.Pagination != null)
                {
                    // ✅ Sadece otomatik mod seçiliyse hesapla
                    if (ConfigControl.Pagination.IsAutoSizeMode)
                    {
                        // Gerçek tableView yüksekliğini kullan
                        double contentHeight = contentControl.ActualHeight;
                        // Eğer tableView varsa, onun yüksekliğini kullan
                        if (contentControl.Content is FrameworkElement contentElement)
                        {
                            contentHeight = contentElement.ActualHeight;
                        }
                        ConfigControl.Pagination.CalculateOptimalPageSize(contentHeight);
                    }
                }
            }
            catch
            {
                // Ignored 
            }
        }
        #region ContentControl
        private void OnContentControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 50 && e.PreviousSize.Height != e.NewSize.Height)
            {
                _resizeTimer?.Stop();
                _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                _resizeTimer.Tick += (s, args) =>
                {
                    _resizeTimer.Stop();
                    CalculateOptimalPageSize();
                };
                _resizeTimer.Start();
            }
        }
        private void OnContentControlContentChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (contentControl?.Content is TableView tableView && ConfigControl?.Selection != null)
            {
                tableView.SelectionChanged -= ConfigControl.Selection.OnSelectionChanged;
                tableView.SelectionChanged += ConfigControl.Selection.OnSelectionChanged;
            }
        }
        #endregion

        #region NotifyPropertyChanged
        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}