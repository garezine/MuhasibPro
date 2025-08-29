using System.Runtime.InteropServices.WindowsRuntime;


using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MuhasibPro.Core.Infrastructure.Tools;

static public class BitmapTools
{
    static public async Task<BitmapImage> LoadBitmapAsync(byte[] bytes)
    {
        if (bytes != null && bytes.Length > 0)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                var bitmap = new BitmapImage();
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await bitmap.SetSourceAsync(stream);
                return bitmap;
            }
        }
        return null;
    }

    static public async Task<BitmapImage> LoadBitmapAsync(IRandomAccessStreamReference randomStreamReference)
    {
        var bitmap = new BitmapImage();
        try
        {
            using (var stream = await randomStreamReference.OpenReadAsync())
            {
                await bitmap.SetSourceAsync(stream);
            }
        }
        catch { }
        return bitmap;
    }
}
