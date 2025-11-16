using Muhasib.Domain.Models;

namespace MuhasibPro.ViewModels.Contracts.Services.CommonServices;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}