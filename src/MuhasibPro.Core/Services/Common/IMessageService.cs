namespace MuhasibPro.Core.Services.Common
{
    public interface IMessageService
    {
        // Subscribe metodları
        void Subscribe<TSender>(object target, Action<TSender, string, object> action) where TSender : class;
        void Subscribe<TSender, TArgs>(object target, Action<TSender, string, TArgs> action) where TSender : class;

        // Unsubscribe metodları
        void Unsubscribe<TSender>(object target) where TSender : class;
        void Unsubscribe<TSender, TArgs>(object target) where TSender : class;
        void Unsubscribe(object target);

        // Send metodları - orijinal sync versiyonlar
        void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;

        // WinUI 3 için async versiyonlar
        Task SendAsync<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
        void RegisterContext(int contextId, IContextService contextService);
        void UnregisterContext(int contextId);
    }
}