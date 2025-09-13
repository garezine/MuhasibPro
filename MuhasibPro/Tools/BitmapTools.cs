using Microsoft.UI.Xaml.Media.Imaging;
using Muhasebe.Business.Services.Abstracts.Common;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace MuhasibPro.Tools
{
    public class BitmapTools : IBitmapTools
    {
        public async Task<object> LoadBitmapAsync(byte[] bytes)
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
    }
}
