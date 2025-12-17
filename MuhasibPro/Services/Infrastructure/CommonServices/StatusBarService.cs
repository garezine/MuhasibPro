using Microsoft.UI.Dispatching;
using Muhasib.Business.Services.Contracts.CommonServices;
using System.ComponentModel;

namespace MuhasibPro.Services.Infrastructure.CommonServices
{
    public class StatusBarService : INotifyPropertyChanged, IStatusBarService
    {
        private DispatcherQueue _dispatcherQueue = null;

        private string _userName;
        private string _databaseConnectionMessage;
        private bool _isDatabaseConnection;

        public IStatusMessageService StatusMessageService { get; set; }

        public StatusBarService(IStatusMessageService statusMessageService)
        {
            StatusMessageService = statusMessageService;
        }

        public void Initialize(object dispatcher)
        {
            _dispatcherQueue = dispatcher as DispatcherQueue;           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Public Properties
        public string UserName
        {
            get => _userName;
            set
            {
                if(_userName != value)
                {
                    _userName = value;
                    NotifyPropertyChanged(nameof(UserName));
                }
            }
        }

        public string DatabaseConnectionMessage
        {
            get => _databaseConnectionMessage;
            set
            {
                if(_databaseConnectionMessage != value)
                {
                    _databaseConnectionMessage = value;
                    NotifyPropertyChanged(nameof(DatabaseConnectionMessage));
                }
            }
        }      

        public bool IsDatabaseConnection
        {
            get => _isDatabaseConnection;
            set
            {
                if(_isDatabaseConnection != value)
                {
                    _isDatabaseConnection = value;
                    NotifyPropertyChanged(nameof(IsDatabaseConnection));
                }
            }
        }        

        public void SetDatabaseStatus(bool isConnected, string message = null)
        {
            ExecuteOnUIThread(
                () =>
                {
                    IsDatabaseConnection = isConnected;
                    if(!string.IsNullOrEmpty(message))
                        DatabaseConnectionMessage = message;
                });
        }
        #endregion

        #region Private Methods
        private void ExecuteOnUIThread(Action action)
        {
            if(_dispatcherQueue != null)
                _dispatcherQueue.TryEnqueue(() => action());
            else
                action();
        }

        public void NotifyPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }
}