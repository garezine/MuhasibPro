using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
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
        public ShellViewModel(IAuthenticationService authenticationService, ICommonServices commonServices)
            : base(commonServices)
        {
            IsLocked = !authenticationService.IsLoggedIn;

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
                    StatusBarService.UserName = $"{UserInfo.Adi} {UserInfo.Soyadi}";
                }
            }
            NavigationService.Navigate(ViewModelArgs.ViewModel, ViewModelArgs.Parameter);
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

        private async void OnMessage(ViewModelBase viewModel, string message, string action)
        {
            switch (message)
            {
                // ✅ SADECE VIEW ENABLE/DISABLE İŞLEMLERİ KALDI

                case "EnableThisView":
                case "DisableThisView":
                    if (viewModel.ContextService.ContextId == ContextService.ContextId)
                    {
                        IsEnabled = message == "EnableThisView";
                        // Status mesajı artık gönderilmiyor - ViewModelBase'ten direkt call
                    }
                    break;

                case "EnableOtherViews":
                case "DisableOtherViews":
                    if (viewModel.ContextService.ContextId != ContextService.ContextId)
                    {
                        await ContextService.RunAsync(() =>
                        {
                            IsEnabled = message == "EnableOtherViews";
                            // Status mesajı artık gönderilmiyor
                        });
                    }
                    break;

                case "EnableAllViews":
                case "DisableAllViews":
                    await ContextService.RunAsync(() =>
                    {
                        IsEnabled = message == "EnableAllViews";
                        // Status mesajı artık gönderilmiyor
                    });
                    break;

                    // ❌ STATUS MESAJLARI TAMAMEN KALDIRILDI
                    // "StatusMessage", "StatusError", "StatusSuccess", "StatusWarning",
                    // "StartProgress", "StartProgressPercent", "UpdateProgress", "StopProgress"
                    // artık ViewModelBase'ten direkt StatusBarService çağrısı yapılacak
            }
        }

        // ❌ SanitizeMessage metodu da kaldırılabilir (artık StatusMessageService içinde)
    }

        /// <summary>
        /// Mesajı temizle - satır sonlarını kaldır
        /// </summary>
     
}