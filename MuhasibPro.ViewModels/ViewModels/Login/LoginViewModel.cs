using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Infrastructure.Models;
using Muhasib.Business.Services.Contracts.AppServices;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Domain.Exceptions;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using MuhasibPro.ViewModels.ViewModels.Dashboard;
using MuhasibPro.ViewModels.ViewModels.Shell;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.Login;

public class LoginViewModel : ViewModelBase
{
    public IAuthenticationService AuthenticationService { get; }

    public IFirmaService FirmaService { get; }

    public LoginViewModel(
        ICommonServices commonServices,
        IAuthenticationService authenticationService,
        IFirmaService firmaService) : base(commonServices)
    {
        AuthenticationService = authenticationService;
        FirmaService = firmaService;
    }


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

    public ICommand LoginWithPasswordCommand => new AsyncRelayCommand(Login);


    private Result ValidateInput()
    {
        if(String.IsNullOrWhiteSpace(Username))
        {
            return Result.Error("Giriş Hatası", "Kullanıcı adı alanı boş geçilemez!");
        }
        if(String.IsNullOrWhiteSpace(Password))
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

    public async Task Login()
    {
        ViewModelArgs.UserInfo = null;
        if(IsLoginWithPassword)
        {
            await LoginWithPassword();
        }
    }


    public async Task LoginWithPassword()
    {
        IsBusy = true;
        var result = ValidateInput();
        if(result.IsOk)
        {
            try
            {
                await AuthenticationService.Login(Username, Password);
                IsBusy = false;
                if(AuthenticationService.IsAuthenticated)
                {
                    ViewModelArgs.UserInfo = AuthenticationService.CurrentAccount;
                    await EnterApplication();
                    return;
                }
            } catch(UserNotFoundException)
            {
                result.Message = "Kullanıcı adı veya şifre hatalı!";
            } catch(InvalidPasswordException)
            {
                result.Message = "Kullanıcı adı veya şifre hatalı!";
            } finally
            {
                IsBusy = false;
            }
        }
        await DialogService.ShowAsync(result.Message, result.Description);
        IsBusy = false;
    }


    private async Task EnterApplication()
    {
        await LogService.SistemLogService
            .SistemLogInformation("Login", "Sisteme giriş yapıldı", "Kullanıcı girişi", $"uygulamaya giriş yapıldı");

        NavigationService.Navigate<FirmaWithMaliDonemSelectViewModel>(ViewModelArgs);
    }
}
