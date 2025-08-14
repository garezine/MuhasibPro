using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MuhasibPro.Core.Infrastructure.Common;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MuhasibPro.Views.Shell;

public sealed partial class ShellStatusBar : UserControl
{
    private DispatcherTimer _timer;
    private StatusBarManager _statusManager;

    public ShellStatusBar()
    {
        InitializeComponent();
        _statusManager = StatusBarManager.Instance;

        // StatusBarManager'dan PropertyChanged olaylarýný dinle
        _statusManager.PropertyChanged += OnStatusManagerPropertyChanged;

        // Baþlangýç deðerlerini ayarla
        UpdateFromStatusManager();

        Timer();
    }

    private void OnStatusManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // UI thread'de güncellemeleri yap
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(StatusBarManager.StatusMessage):
                    StatusMessage = _statusManager.StatusMessage;
                    break;
                case nameof(StatusBarManager.UserName):
                    UserName = _statusManager.UserName;
                    break;
                case nameof(StatusBarManager.DatabaseConnectionMessage):
                    DatabaseConnectionMessage = _statusManager.DatabaseConnectionMessage;
                    break;
                case nameof(StatusBarManager.IsError):
                    IsError = _statusManager.IsError;
                    break;
                case nameof(StatusBarManager.IsSaveStatus):
                    IsSaveStatus = _statusManager.IsSaveStatus;
                    break;
                case nameof(StatusBarManager.IsStatusProgress):
                    IsStatusProgress = _statusManager.IsStatusProgress;
                    break;
                case nameof(StatusBarManager.IsDatabaseConnection):
                    IsDatabaseConnection = _statusManager.IsDatabaseConnection;
                    break;
            }
        });
    }

    private void UpdateFromStatusManager()
    {
        StatusMessage = _statusManager.StatusMessage;
        UserName = _statusManager.UserName;
        DatabaseConnectionMessage = _statusManager.DatabaseConnectionMessage;
        IsError = _statusManager.IsError;
        IsSaveStatus = _statusManager.IsSaveStatus;
        IsStatusProgress = _statusManager.IsStatusProgress;
        IsDatabaseConnection = _statusManager.IsDatabaseConnection;
    }

    private void Timer()
    {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) =>
        {
            TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss");
            TimeDisplay.Visibility = Visibility.Visible;
        };
        _timer.Start();
    }

    // Dispose pattern for cleanup
    ~ShellStatusBar()
    {
        if (_statusManager != null)
        {
            _statusManager.PropertyChanged -= OnStatusManagerPropertyChanged;
        }

        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
    }

    #region Dependency Properties (StatusBarManager ile senkronize)

    #region StatusMessage
    public string StatusMessage
    {
        get => (string)GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    public static readonly DependencyProperty StatusMessageProperty =
        DependencyProperty.Register(nameof(StatusMessage), typeof(string), typeof(ShellStatusBar),
            new PropertyMetadata("Hazýr"));
    #endregion

    #region UserName
    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public static readonly DependencyProperty UserNameProperty =
        DependencyProperty.Register(nameof(UserName), typeof(string), typeof(ShellStatusBar),
            new PropertyMetadata(null));
    #endregion

    #region DatabaseConnectionMessage
    public string DatabaseConnectionMessage
    {
        get => (string)GetValue(DatabaseConnectionMessageProperty);
        set => SetValue(DatabaseConnectionMessageProperty, value);
    }

    public static readonly DependencyProperty DatabaseConnectionMessageProperty =
        DependencyProperty.Register(nameof(DatabaseConnectionMessage), typeof(string), typeof(ShellStatusBar),
            new PropertyMetadata(null));
    #endregion

    #region IsError
    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public static readonly DependencyProperty IsErrorProperty =
        DependencyProperty.Register(nameof(IsError), typeof(bool), typeof(ShellStatusBar),
            new PropertyMetadata(false));
    #endregion

    #region IsSaveStatus
    public bool IsSaveStatus
    {
        get => (bool)GetValue(IsSaveStatusProperty);
        set => SetValue(IsSaveStatusProperty, value);
    }

    public static readonly DependencyProperty IsSaveStatusProperty =
        DependencyProperty.Register(nameof(IsSaveStatus), typeof(bool), typeof(ShellStatusBar),
            new PropertyMetadata(false));
    #endregion

    #region IsStatusProgress
    public bool IsStatusProgress
    {
        get => (bool)GetValue(IsStatusProgressProperty);
        set => SetValue(IsStatusProgressProperty, value);
    }

    public static readonly DependencyProperty IsStatusProgressProperty =
        DependencyProperty.Register(nameof(IsStatusProgress), typeof(bool), typeof(ShellStatusBar),
            new PropertyMetadata(false));
    #endregion

    #region IsDatabaseConnection
    public bool IsDatabaseConnection
    {
        get => (bool)GetValue(IsDatabaseConnectionProperty);
        set => SetValue(IsDatabaseConnectionProperty, value);
    }

    public static readonly DependencyProperty IsDatabaseConnectionProperty =
        DependencyProperty.Register(nameof(IsDatabaseConnection), typeof(bool), typeof(ShellStatusBar),
            new PropertyMetadata(false));
    #endregion

    public bool StatusFontIcon => IsError;

    #endregion
}
