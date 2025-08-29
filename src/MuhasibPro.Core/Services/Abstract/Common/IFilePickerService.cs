namespace MuhasibPro.Core.Services.Abstract.Common;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}