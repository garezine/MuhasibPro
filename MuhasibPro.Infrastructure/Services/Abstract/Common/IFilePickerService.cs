namespace MuhasibPro.Infrastructure.Services.Abstract.Common;

public interface IFilePickerService
{
    Task<ImagePickerResult> OpenImagePickerAsync();
}