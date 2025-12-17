using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Muhasib.Business.Services.Contracts.CommonServices;
using MuhasibPro.Converters;
using MuhasibPro.HostBuilders;
using System.ComponentModel;


namespace MuhasibPro.Views.Shell
{
    public sealed partial class ShellStatusBar : UserControl, IDisposable, INotifyPropertyChanged
    {
        private readonly IStatusBarService _statusService;
        private DispatcherTimer _timer;

        // WRAPPER PROPERTIES - XAML'in anlayacağı dil
        public string StatusMessage => _statusService.StatusMessageService.StatusMessage;
        public string StatusIconGlyph => _statusService.StatusMessageService.StatusIconGlyph;
        public string StatusColorHex => _statusService.StatusMessageService.StatusColorHex;
        public bool ShowStatusIcon => _statusService.StatusMessageService.ShowStatusIcon;
        public bool IsProgressVisible => _statusService.StatusMessageService.IsProgressVisible;
        public bool IsProgressIndeterminate => _statusService.StatusMessageService.IsProgressIndeterminate;
        public double ProgressValue => _statusService.StatusMessageService.ProgressValue;
        public string ProgressText => _statusService.StatusMessageService.ProgressText;
        public bool ShowProgressBar => _statusService.StatusMessageService.ShowProgressBar;

        // StatusBarService properties - direkt
        public string UserName => _statusService.UserName;
        public string DatabaseConnectionMessage => _statusService.DatabaseConnectionMessage;
        
        public bool IsDatabaseConnection => _statusService.IsDatabaseConnection;

        public ShellStatusBar()
        {
            InitializeComponent();

            _statusService = ServiceLocator.Current.GetService<IStatusBarService>();

            // PropertyChanged event'lerini dinle
            _statusService.PropertyChanged += OnServicePropertyChanged;
            _statusService.StatusMessageService.PropertyChanged += OnServicePropertyChanged;

            this.DataContext = this; // KENDİMİZ!

            InitializeTimer();
        }

        private void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NotifyPropertyChanged(e.PropertyName);

                // Computed property'ler için daha spesifik
                switch (e.PropertyName)
                {
                    case nameof(IStatusMessageService.StatusColorHex):
                        NotifyPropertyChanged(nameof(StatusBrush));
                        break;
                    case nameof(IStatusMessageService.StatusIconGlyph):
                        NotifyPropertyChanged(nameof(StatusGlyph));
                        break;
                    case nameof(IStatusMessageService.IsProgressVisible):
                    case nameof(Muhasib.Business.Services.Contracts.CommonServices.IStatusMessageService.IsProgressIndeterminate):
                        NotifyPropertyChanged(nameof(ShowProgressBarVisibility));
                        break;
                }
            });
        }

        // Computed properties (XAML için)
        public SolidColorBrush StatusBrush => new SolidColorBrush(ColorConverter.Parse(StatusColorHex));
        public string StatusGlyph => StatusIconGlyph;
        public Visibility ShowProgressBarVisibility => ShowProgressBar ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowUserInfoVisibility => !string.IsNullOrEmpty(UserName) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ShowDatabaseInfoVisibility => !string.IsNullOrEmpty(DatabaseConnectionMessage) ? Visibility.Visible : Visibility.Collapsed;
        public SolidColorBrush DatabaseIconBrush => IsDatabaseConnection ?
            new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.OrangeRed);

        private void InitializeTimer()
        {
          
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss");
            _timer.Start();
        }

        public void Dispose()
        {
            _statusService.PropertyChanged -= OnServicePropertyChanged;
            _statusService.StatusMessageService.PropertyChanged -= OnServicePropertyChanged;
            _timer?.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}