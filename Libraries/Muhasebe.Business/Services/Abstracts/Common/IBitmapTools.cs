namespace Muhasebe.Business.Services.Abstracts.Common
{
    public interface IBitmapTools
    {
        Task<object> LoadBitmapAsync(byte[] bytes);
    }
}
