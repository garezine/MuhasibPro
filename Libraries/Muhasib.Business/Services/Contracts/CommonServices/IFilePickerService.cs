using Muhasib.Domain.Models;

namespace Muhasib.Business.Services.Contracts.CommonServices;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}