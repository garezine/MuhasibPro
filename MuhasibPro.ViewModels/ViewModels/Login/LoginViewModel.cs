using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Login;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    public LoginViewModel(ICommonServices commonServices, IAuthenticationService authenticationService) : base(
        commonServices)
    { _authenticationService = authenticationService; }

    public string LastUpdateDate => DateTime.Now.ToShortDateString();

    private string _username = "korkutomer";
    private string _password = "Ok241341";

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            Set(ref _username, value);
        }
    }
    public async Task LoadAsync(ShellArgs args)
    {
        ViewModelArgs = args;
        IsLoginWithPassword = true;
        IsBusy = false;
        await Task.CompletedTask;

    }

    public string Password { get => _password; set => Set(ref _password, value); }

    private ShellArgs ViewModelArgs { get; set; }
    public ICommand LoginWithPasswordCommand => new RelayCommand(LoginWithPassword);


    private Result ValidateInput()
    {
        if (String.IsNullOrWhiteSpace(Username))
        {
            return Result.Error("Giriş Hatası", "Kullanıcı adı alanı boş geçilemez!");
        }
        if (String.IsNullOrWhiteSpace(Password))
        {
            return Result.Error("Giriş Hatası", "Şifre alanı boş geçilemez!");
        }
        return Result.Ok();
    }

    private bool _isLoginWithPassword = false;

    public bool IsLoginWithPassword
    {
        get { return _isLoginWithPassword; }
        set { Set(ref _isLoginWithPassword, value); }
    }

    public void Login()
    {
        if (IsLoginWithPassword)
        {
            LoginWithPassword();
        }
    }

    public async void LoginWithPassword()
    {
        IsBusy = true;
        var result = ValidateInput();
        if (result.IsOk)
        {
            try
            {
                await _authenticationService.Login(Username, Password);
                IsBusy = false;
                if (_authenticationService.IsLoggedIn)
                {
                    await EnterApplication();
                    return;
                }
            }
            catch (UserNotFoundException)
            {
                result.Message = "Kullanıcı adı veya şifre hatalı!";
            }
            catch (InvalidPasswordException)
            {
                result.Message = "Kullanıcı adı veya şifre hatalı!";
            }
            finally
            {
                IsBusy = false;
            }
        }
        await DialogService.ShowAsync(result.Message, result.Description);
        IsBusy = false;
    }

    private async Task EnterApplication()
    {
        if (ViewModelArgs.UserInfo.KullaniciAdi == Username)
        {
            ViewModelArgs.UserInfo = _authenticationService.CurrentAccount;
            await LogService.SistemLogService.SistemLogInformation("Login", "Sisteme giriş yapıldı", "Kullanıcı girişi", $"uygulamaya giriş yapıldı");
        }
        NavigationService.Navigate<MainShellViewModel>(ViewModelArgs);
        
    }
}
