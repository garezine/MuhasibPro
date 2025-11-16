namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
