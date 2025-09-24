using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using MuhasibPro.ViewModels.Contracts.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.ViewModels.Shell
{
    public class ShellArgs
    {
        public Type ViewModel { get; set; }
        public object Parameter { get; set; }
        public KullaniciModel UserInfo { get; set; }
    }
    public class ShellViewModel : ViewModelBase
    {
        public ShellViewModel(IAuthenticationService authenticationService, ICommonServices commonServices) : base(commonServices)
        {
        }
        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set => Set(ref _isLocked, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => Set(ref _isEnabled, value);
        }
        private bool _isSaveStatus = false;
        public bool IsSaveStatus
        {
            get => _isSaveStatus;
            set => Set(ref _isSaveStatus, value);
        }


        private bool _isError = false;
        public bool IsError
        {
            get => _isError;
            set => Set(ref _isError, value);
        }

        public KullaniciModel UserInfo
        {
            get; protected set;
        }

        public ShellArgs ViewModelArgs
        {
            get; protected set;
        }

        public virtual Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            if (ViewModelArgs != null)
            {
                UserInfo = ViewModelArgs.UserInfo;
                NavigationService.Navigate(ViewModelArgs.ViewModel, ViewModelArgs.Parameter);
            }
            return Task.CompletedTask;
        }
        public virtual void Unload()
        {
        }

        public virtual void Subscribe()
        {
            MessageService.Subscribe<IAuthenticationService, bool>(this, OnLoginMessage);
            MessageService.Subscribe<ViewModelBase, string>(this, OnMessage);
        }

        public virtual void Unsubscribe()
        {
            MessageService.Unsubscribe(this);
        }

        private async void OnLoginMessage(IAuthenticationService loginService, string message, bool isAuthenticated)
        {
            if (message == "AuthenticationChanged")
            {
                await ContextService.RunAsync(() =>
                {
                    IsLocked = !isAuthenticated;
                });
            }
        }

        private async void OnMessage(ViewModelBase viewModel, string message, string status)
        {
            switch (message)
            {
                case "StatusMessage":
                case "StatusError":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsError = message == "StatusError";
                        SetStatus(status);
                    }
                    break;

                case "EnableThisView":
                case "DisableThisView":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsEnabled = message == "EnableThisView";
                        SetStatus(status);
                    }
                    break;

                case "EnableOtherViews":
                case "DisableOtherViews":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        await ContextService.RunAsync(() =>
                        {
                            IsEnabled = message == "EnableOtherViews";
                            SetStatus(status);
                        });
                    }
                    break;

                case "EnableAllViews":
                case "DisableAllViews":
                    await ContextService.RunAsync(() =>
                    {
                        IsEnabled = message == "EnableAllViews";
                        SetStatus(status);
                    });
                    break;
            }
        }

        private void SetStatus(string message)
        {
            message = message ?? string.Empty;
            message = message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            StatusBar.StatusMessage = message;
        }
    }
}
