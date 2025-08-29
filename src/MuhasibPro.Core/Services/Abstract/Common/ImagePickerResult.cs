namespace MuhasibPro.Core.Services.Abstract.Common
{
    public class ImagePickerResult
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] ImageBytes { get; set; }

        public object ImageSource { get; set; }
    }
}
