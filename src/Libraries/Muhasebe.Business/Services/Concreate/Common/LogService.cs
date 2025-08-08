using Muhasebe.Business.Models.DbModel.Logs;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Interfaces.App;

namespace Muhasebe.Business.Services.Concreate.Common
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;


        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;

        }

        public async Task<int> DeleteLogAsync(long id)
        {
            return await _logRepository.DeleteLogAsync(id);
        }

        public async Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<AppLog> request)
        {
            return await _logRepository.DeleteLogRangeAsync(index, length, request);
        }

        public async Task<AppLogModel> GetLogAsync(long id)
        {
            var log = await _logRepository.GetLogAsync(id);
            if (log != null)
            {
                return CreateAppLogModel(log);
            }
            return null;
        }

        public async Task<IList<AppLogModel>> GetLogsAsync()
        {
            var models = new List<AppLogModel>();
            var logs = await _logRepository.GetLogsAsync();
            foreach (var log in logs)
            {
                models.Add(CreateAppLogModel(log));
            }
            return models;
        }

        public async Task<IList<AppLogModel>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request)
        {
            var models = new List<AppLogModel>();
            var logs = await _logRepository.GetLogsAsync(skip, take, request);
            foreach (var log in logs)
            {
                models.Add(CreateAppLogModel(log));
            }
            return models;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<AppLog> request)
        {
            return await _logRepository?.GetLogsCountAsync(request);
        }

        public async Task LogErrorAsync(string source, string action, string message, string description = "")
        {
            await _logRepository.WriteAsync(LogType.Hata, source, action, message, description);
        }

        public async Task LogExceptionAsync(string source, string action, Exception ex)
        {
            await _logRepository.WriteAsync(LogType.Hata, source, action, ex).ConfigureAwait(false);
        }

        public async Task LogInformationAsync(string source, string action, string message, string description = "")
        {
            await _logRepository.WriteAsync(LogType.Bilgi, source, action, message, description).ConfigureAwait(false);
        }

        public async Task MarkAllAsReadAsync()
        {
            await _logRepository.MarkAllAsReadAsync();
        }

        public async Task WriteAsync(LogType type, string source, string action, string message, string description)
        {
            await _logRepository.WriteAsync(type, source, action, message, description).ConfigureAwait(false);
        }

        public async Task WriteAsync(LogType type, string source, string action, Exception ex)
        {
            var description = ex.ToString(); // Tüm inner exception'ları içerir
            await WriteAsync(LogType.Hata, source, action, ex.Message, description).ConfigureAwait(false);
        }
        private AppLogModel CreateAppLogModel(AppLog source)
        {
            return new AppLogModel()
            {
                Id = source.Id,
                IsRead = source.IsRead,
                KayitTarihi = source.KayitTarihi,
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

