using MuhasibPro.ViewModels.Helpers;

namespace MuhasibPro.ViewModels.Contracts.CommonServices;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}