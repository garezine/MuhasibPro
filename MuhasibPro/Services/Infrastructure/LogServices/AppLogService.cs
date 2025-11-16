using Muhasib.Business.Models.MuhasebeModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using Muhasib.Domain.Enum;
using MuhasibPro.ViewModels.Contracts.Services.CommonServices;

namespace MuhasibPro.Services.Infrastructure.LogServices
{
    public class AppLogService : IAppLogService
    {
        private readonly IAppLogRepository _logRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthenticationService _authenticationService;
        public AppLogService(IAppLogRepository appLogRepository, IMessageService messageService, IAuthenticationService authenticationService)
        {
            _logRepository = appLogRepository;
            _messageService = messageService;
            _authenticationService = authenticationService;
        }
        public async Task AppLogMarkAllAsReadAsync() { await _logRepository.MarkAllAsReadAsync(); }
        public async Task<int> DeleteAppLogAsync(AppLogModel model)
        {
            var log = new AppLog { Id = model.Id };
            return await _logRepository.DeleteLogsAsync(log);
        }
        public async Task<int> DeleteAppLogRangeAsync(int index, int length, DataRequest<AppLog> request)
        {
            var items = await _logRepository.GetLogKeysAsync(index, length, request);
            return await _logRepository.DeleteLogsAsync(items.ToArray());
        }
        public async Task<AppLogModel> GetAppLogAsync(long id)
        {
            var item = await _logRepository.GetLogAsync(id);
            if (item == null)
                return null;
            return CreateAppLogModel(item);
        }
        public async Task<IList<AppLogModel>> GetAppLogsAsync(int skip, int take, DataRequest<AppLog> request)
        {
            var models = new List<AppLogModel>();
            var items = await _logRepository.GetLogsAsync(skip, take, request);
            foreach (var item in items)
            {
                models.Add(CreateAppLogModel(item));
            }
            return models;
        }
        public async Task<int> GetAppLogsCountAsync(DataRequest<AppLog> request)
        { return await _logRepository.GetLogsCountAsync(request); }
        public async Task WriteAsync(LogType type, string source, string action, string message, string description)
        {
            var appLog = new AppLog()
            {
                User = _authenticationService!.CurrentUsername,
                KaydedenId = _authenticationService.CurrentUserId,
                Type = type,
                Source = source,
                Action = action,
                Message = message,
                Description = description,
            };
            appLog.IsRead = type != LogType.Hata;
            await CreateLogAsync(appLog);
            _messageService.Send(this, "LogAdded", appLog);
        }
        public async Task<int> CreateLogAsync(AppLog appLog) { return await _logRepository.CreateLogAsync(appLog); }
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
        private AppLogModel CreateAppLogModel(AppLog source)
        {
            return new AppLogModel()
            {
                Id = source.Id,
                IsRead = source.IsRead,
                KayitTarihi = source.KayitTarihi,
                KaydedenId = source.KaydedenId,
                AktifMi = source.AktifMi,
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
