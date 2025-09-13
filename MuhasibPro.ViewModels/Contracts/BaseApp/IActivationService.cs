namespace MuhasibPro.ViewModels.Contracts.BaseApp;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
