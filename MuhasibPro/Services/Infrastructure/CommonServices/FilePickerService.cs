using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.UtilityServices;
using Muhasib.Domain.Models;
using MuhasibPro.Helpers;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace MuhasibPro.Services.Infrastructure.CommonServices;
public class FilePickerService : IFilePickerService
{
    private readonly IBitmapToolsService _bitmapTools;

    public FilePickerService(IBitmapToolsService bitmapTools)
    {
        _bitmapTools = bitmapTools;
    }

    public async Task<ImagePickerResult> OpenImagePickerAsync()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        var window = CustomWindowHelper.CurrentWindow;
        var hwnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(picker, hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            var bytes = await GetImageBytesAsync(file);
            return new ImagePickerResult
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                ImageBytes = bytes,
                ImageSource = await _bitmapTools.LoadBitmapAsync(bytes)
            };
        }
        return null;
    }

    private static async Task<byte[]> GetImageBytesAsync(StorageFile file)
    {
        using (var randomStream = await file.OpenReadAsync())
        {
            using (var stream = randomStream.AsStream())
            {
                byte[] buffer = new byte[randomStream.Size];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}

