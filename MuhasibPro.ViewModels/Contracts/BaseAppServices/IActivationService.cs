namespace MuhasibPro.ViewModels.Contracts.BaseAppServices;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
