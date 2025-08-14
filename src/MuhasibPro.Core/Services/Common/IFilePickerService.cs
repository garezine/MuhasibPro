namespace MuhasibPro.Core.Services.Common;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}