namespace Muhasib.Business.Services.Contracts.CommonServices;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);

}
