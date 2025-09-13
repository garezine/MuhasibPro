using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Helpers;

namespace Muhasebe.Business.Services.Concrete.Common
{
    public class SistemLogService : ISistemLogService
    {
        private readonly ISistemLogRepository _logRepository;
        private readonly IAuthenticationService _authenticator;

        public SistemLogService(ISistemLogRepository logRepository, IAuthenticationService authenticator)
        {
            _logRepository = logRepository;
            _authenticator = authenticator;
        }

        public Task<int> CreateLogAsync(SistemLog appLog)
        {
            return _logRepository.CreateLogAsync(appLog);
        }

        public async Task<int> DeleteLogAsync(SistemLogModel model)
        {
            var sistemLog = new SistemLog { Id = model.Id };
            return await _logRepository.DeleteLogsAsync(sistemLog);
        }

        public async Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<SistemLog> request)
        {
            var items = await _logRepository.GetLogKeysAsync(index, length, request);
            return await _logRepository.DeleteLogsAsync(items.ToArray());
        }

        public async Task<SistemLogModel> GetLogAsync(long id)
        {
            var item = await _logRepository.GetLogAsync(id);
            if (item != null)
            {
                return CreateSistemLogModel(item);
            }
            return null;
        }

        public async Task<IList<SistemLogModel>> GetLogsAsync(int skip, int take, DataRequest<SistemLog> request)
        {
            var models = new List<SistemLogModel>();
            var items = await _logRepository.GetLogsAsync(skip, take, request).ConfigureAwait(false);
            foreach (var item in items)
            {
                models.Add(CreateSistemLogModel(item));
            }
            return models;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<SistemLog> request)
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
            var sistemLog = new SistemLog()
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
        private SistemLogModel CreateSistemLogModel(SistemLog source)
        {
            return new SistemLogModel()
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

