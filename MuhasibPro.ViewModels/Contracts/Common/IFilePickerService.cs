using MuhasibPro.ViewModels.Helpers;

namespace MuhasibPro.ViewModels.Contracts.Common;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}