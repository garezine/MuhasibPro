using Muhasebe.Business.Models.DegerlerModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Business.Services.Concrete.Common
{
    public class AppLogService : IAppLogService
    {
        private readonly IAppLogRepository _logRepository;
        private readonly IAuthenticationService _authenticator;

        public AppLogService(IAppLogRepository logRepository, IAuthenticationService authenticator)
        {
            _logRepository = logRepository;
            _authenticator = authenticator;
        }
        public Task<int> CreateLogAsync(AppLog appLog)
        {
            return _logRepository.CreateLogAsync(appLog);
        }

        public async Task<int> DeleteLogAsync(AppLogModel model)
        {
            var sistemLog = new AppLog { Id = model.Id };
            return await _logRepository.DeleteLogsAsync(sistemLog);
        }

        public async Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<AppLog> request)
        {
            var items = await _logRepository.GetLogKeysAsync(index, length, request);
            return await _logRepository.DeleteLogsAsync(items.ToArray());
        }

        public async Task<AppLogModel> GetLogAsync(long id)
        {
            var item = await _logRepository.GetLogAsync(id);
            if (item != null)
            {
                return CreateSistemLogModel(item);
            }
            return null;
        }

        public async Task<IList<AppLogModel>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request)
        {
            var models = new List<AppLogModel>();
            var items = await _logRepository.GetLogsAsync(skip, take, request).ConfigureAwait(false);
            foreach (var item in items)
            {
                models.Add(CreateSistemLogModel(item));
            }
            return models;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<AppLog> request)
        {
            return await _logRepository.GetLogsCountAsync(request);
        }

        public async Task MarkAllAsReadAsync()
        {
            await _logRepository.MarkAllAsReadAsync();
        }

        public async Task WriteAsync(LogType type, string source, string action, Exception ex)
        {
            await WriteAsync(LogType.Hata, source, action, ex.Message, ex.ToString());
            Exception deepException = ex.InnerException;
            while (deepException != null)
            {
                await WriteAsync(LogType.Hata, source, action, deepException.Message, deepException.ToString());
                deepException = deepException.InnerException;
            }
        }

        public async Task WriteAsync(LogType type, string source, string action, string message, string description)
        {
            var sistemLog = new AppLog()
            {
                User = _authenticator?.CurrentUsername ?? "Sistem",
                KaydedenId = _authenticator?.CurrentUserId ?? 0,
                Type = type,
                Source = source,
                Action = action,
                Message = message,
                Description = description,
            };
            sistemLog.IsRead = type != LogType.Hata;
            await CreateLogAsync(sistemLog);
        }
        private AppLogModel CreateSistemLogModel(AppLog source)
        {
            return new AppLogModel()
            {
                Id = source.Id,
                IsRead = source.IsRead,
                KayitTarihi = source.KayitTarihi,
                KaydedenId = source.KaydedenId,
                User = source.User,
                Type = source.Type,
                Source = source.Source,
                Action = source.Action,
                Message = source.Message,
                Description = source.Description,
            };
        }
    }
}

