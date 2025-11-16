namespace Muhasib.Business.Services.Contracts.UtilityServices
{
    public interface IBitmapToolsService
    {
        Task<object> LoadBitmapAsync(byte[] bytes);
        Lazy<Task<object>> CreateLazyImageLoader(byte[] imageData);
    }
}
