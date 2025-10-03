namespace MuhasibPro.Contracts.BaseAppServices;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
