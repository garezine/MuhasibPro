using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using MuhasibPro.Contracts.CommonServices;
using MuhasibPro.Infrastructure.ViewModels;

namespace MuhasibPro.ViewModels.Shell
{
    public class ShellArgs
    {
        public Type ViewModel { get; set; }
        public object Parameter { get; set; }
        public KullaniciModel UserInfo { get; set; }
    }

    public class ShellViewModel : ViewModelBase
    {
        public ShellViewModel(IAuthenticationService authenticationService, ICommonServices commonServices)
            : base(commonServices)
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

        public KullaniciModel UserInfo { get; protected set; }

        public ShellArgs ViewModelArgs { get; protected set; }

        public virtual Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            if (ViewModelArgs != null)
            {
                UserInfo = ViewModelArgs.UserInfo;

                // Kullanıcı bilgisini StatusBar'a aktar
                if (UserInfo != null)
                {
                    StatusBar.UserName = $"{UserInfo.Adi} {UserInfo.Soyadi}";
                }

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
                // Status mesajları - yeni tip destekli
                case "StatusMessage":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        StatusBar.ShowInfo(SanitizeMessage(status));
                    }
                    break;

                case "StatusError":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        StatusBar.ShowError(SanitizeMessage(status));
                    }
                    break;

                case "StatusSuccess":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        StatusBar.ShowSuccess(SanitizeMessage(status));
                    }
                    break;

                case "StatusWarning":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        StatusBar.ShowWarning(SanitizeMessage(status));
                    }
                    break;

                // View enable/disable
                case "EnableThisView":
                case "DisableThisView":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsEnabled = message == "EnableThisView";
                        if (!string.IsNullOrEmpty(status))
                        {
                            StatusBar.ShowInfo(SanitizeMessage(status));
                        }
                    }
                    break;

                case "EnableOtherViews":
                case "DisableOtherViews":
                    if (viewModel.ContextService.ContextId != ContextService.ContextId)
                    {
                        await ContextService.RunAsync(() =>
                        {
                            IsEnabled = message == "EnableOtherViews";
                            if (!string.IsNullOrEmpty(status))
                            {
                                StatusBar.ShowInfo(SanitizeMessage(status));
                            }
                        });
                    }
                    break;

                case "EnableAllViews":
                case "DisableAllViews":
                    await ContextService.RunAsync(() =>
                    {
                        IsEnabled = message == "EnableAllViews";
                        if (!string.IsNullOrEmpty(status))
                        {
                            StatusBar.ShowInfo(SanitizeMessage(status));
                        }
                    });
                    break;
            }
        }

        /// <summary>
        /// Mesajı temizle - satır sonlarını kaldır
        /// </summary>
        private string SanitizeMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            return message
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }
    }
}