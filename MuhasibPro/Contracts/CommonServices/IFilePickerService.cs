using MuhasibPro.Helpers;

namespace MuhasibPro.Contracts.CommonServices;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}