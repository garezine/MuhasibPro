using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
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
        private readonly IAuthenticationService _authenticationService;
        public ShellViewModel(IAuthenticationService authenticationService, ICommonServices commonServices) : base(
            commonServices)
        {
            _authenticationService = authenticationService;
            UpdateLockedStatus();
            _authenticationService.StateChanged += OnAuthenticationStateChanged;
        }
        private void OnAuthenticationStateChanged()
        {
            ContextService.RunAsync(() =>
            {
                UpdateLockedStatus();

                if (!_authenticationService.IsAuthenticated)
                {
                    UserInfo = null;
                    StatusBarService.UserName = string.Empty;
                }
                else
                {
                    // Eğer gerekliyse, CurrentAccount'dan UserInfo'yu güncelle
                    UserInfo = _authenticationService.CurrentAccount;
                    if (UserInfo != null)
                    {
                        StatusBarService.UserName = $"{UserInfo.Adi} {UserInfo.Soyadi}";
                    }
                }
            });
        }
        private bool _isLocked = false;

        public bool IsLocked { get => _isLocked; set => Set(ref _isLocked, value); }

        private bool _isEnabled = true;

        public bool IsEnabled { get => _isEnabled; set => Set(ref _isEnabled, value); }

        public KullaniciModel UserInfo { get; protected set; }
        public FirmaModel SelectedFirma { get; protected set; }
        public MaliDonemModel SelectedMaliDonem { get; protected set; }

        public ShellArgs ViewModelArgs { get; protected set; }       

        public virtual Task LoadAsync(ShellArgs args)
        {
            ViewModelArgs = args;
            if(ViewModelArgs != null)
            {
                UserInfo = ViewModelArgs.UserInfo;
               
                // Kullanıcı bilgisini StatusBar'a aktar
                if(UserInfo != null)
                {
                    StatusBarService.UserName = $"{UserInfo.Adi} {UserInfo.Soyadi}";
                    UpdateLockedStatus();
                }
            }
            NavigationService.Navigate(ViewModelArgs.ViewModel, ViewModelArgs.Parameter);
            return Task.CompletedTask;
        }

        public virtual void Unload()
        {
            if (_authenticationService != null)
            {
                _authenticationService.StateChanged -= OnAuthenticationStateChanged;
            }
        }
        public void Logout()
        {
            _authenticationService.Logout();
            UserInfo = null;
        }
        public virtual void Subscribe()
        {
            MessageService.Subscribe<IAuthenticationService, bool>(this, OnLoginMessage);
            MessageService.Subscribe<ViewModelBase, string>(this, OnMessage);
        }
        private void UpdateLockedStatus()
        {
            IsLocked = !_authenticationService.IsAuthenticated;
        }
        public virtual void Unsubscribe() { MessageService.Unsubscribe(this); }

        private async void OnLoginMessage(IAuthenticationService loginService, string message, bool isAuthenticated)
        {
            if (message == "AuthenticationChanged")
            {
                await ContextService.RunAsync(() =>
                {
                    UpdateLockedStatus();

                    if (!isAuthenticated)
                    {
                        UserInfo = null;
                        StatusBarService.UserName = string.Empty;
                    }
                    else if (UserInfo == null && _authenticationService.CurrentAccount != null)
                    {
                        UserInfo = _authenticationService.CurrentAccount;
                        StatusBarService.UserName = $"{UserInfo.Adi} {UserInfo.Soyadi}";
                    }
                });
            }
        }

        private async void OnMessage(ViewModelBase viewModel, string message, string action)
        {
            switch(message)
            {
                // ✅ SADECE VIEW ENABLE/DISABLE İŞLEMLERİ KALDI

                case "EnableThisView":
                case "DisableThisView":
                    if(viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsEnabled = message == "EnableThisView";
                        // Status mesajı artık gönderilmiyor - ViewModelBase'ten direkt call
                    }
                    break;

                case "EnableOtherViews":
                case "DisableOtherViews":
                    if(viewModel.ContextService.ContextId != ContextService.ContextId)
                    {
                        await ContextService.RunAsync(
                            () =>
                            {
                                IsEnabled = message == "EnableOtherViews";
                                // Status mesajı artık gönderilmiyor
                            });
                    }
                    break;

                case "EnableAllViews":
                case "DisableAllViews":
                    await ContextService.RunAsync(
                        () =>
                        {
                            IsEnabled = message == "EnableAllViews";
                            // Status mesajı artık gönderilmiyor
                        });
                    break;
                
            }
        }
    }
}
