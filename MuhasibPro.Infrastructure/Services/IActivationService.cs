namespace MuhasibPro.Infrastructure.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs); 
}
