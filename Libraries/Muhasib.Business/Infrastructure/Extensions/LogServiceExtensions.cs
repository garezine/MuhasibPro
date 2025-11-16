using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Domain.Enum;

namespace Muhasib.Business.Infrastructure.Extensions
{
    public static class LogServiceExtensions
    {
        //ISistemLogService extensions methods
        public static async Task SistemLogInformation(this ISistemLogService sistemLogService, string source, string action, string message, string description)
        { await sistemLogService.WriteAsync(LogType.Bilgi, source, action, message, description); }
        public static async Task SistemLogError(this ISistemLogService sistemLogService, string source, string action, string message, string description = "")
        { await sistemLogService.WriteAsync(LogType.Hata, source, action, message, description); }
        public static async Task SistemLogException(this ISistemLogService sistemLogService, string source, string action, Exception exception)
        { await sistemLogService.WriteAsync(LogType.Hata, source, action, exception.Message, exception.ToString()); }

        // IAppLogService extension methods

        public static async void AppLogInformation(this IAppLogService appLogService, string source, string action, string message, string description)
        { await appLogService.WriteAsync(LogType.Bilgi, source, action, message, description); }
        public static async void AppLogError(this IAppLogService appLogService, string source, string action, Exception exception)
        { await appLogService.WriteAsync(LogType.Hata, source, action, exception.Message, exception.ToString()); }
        public static async void AppLogException(this IAppLogService appLogService, string source, string action, Exception exception)
        { await appLogService.WriteAsync(LogType.Hata, source, action, exception.Message, exception.ToString()); }
    }
}
