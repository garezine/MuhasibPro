using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.CommonServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Domain.Entities.SistemEntity;
using Muhasib.Domain.Enum;

namespace MuhasibPro.Services.Infrastructure.LogServices
{
    public class SistemLogService : ISistemLogService
    {
        private readonly ISistemLogRepository _logRepository;
        private readonly IMessageService _messageService;
        private readonly IAuthenticationService _authenticationService;
        public SistemLogService(ISistemLogRepository sistemLogRepository, IMessageService messageService, IAuthenticationService authenticationService)
        {
            _logRepository = sistemLogRepository;
            _messageService = messageService;
            _authenticationService = authenticationService;
        }

        public async Task<int> DeleteSistemLogAsync(SistemLogModel model)
        {
            var existingLog = await _logRepository.GetByIdAsync(model.Id);
            if (existingLog != null)
            {
                return await _logRepository.DeleteLogsAsync(existingLog);
            }
            return 0;
        }

        public async Task<int> DeleteSistemLogRangeAsync(int index, int length, DataRequest<SistemLog> request)
        {
            var items = await _logRepository.GetLogKeysAsync(index, length, request);
            return await _logRepository.DeleteLogsAsync(items.ToArray());
        }

        public async Task<SistemLogModel> GetSistemLogAsync(long id)
        {
            var item = await _logRepository.GetLogAsync(id);
            if (item == null)
                return null;
            return CreateSistemLogModel(item);
        }

        public async Task<IList<SistemLogModel>> GetSistemLogsAsync(int skip, int take, DataRequest<SistemLog> request)
        {
            var models = new List<SistemLogModel>();
            var items = await _logRepository.GetLogsAsync(skip, take, request);
            foreach (var item in items)
            {
                models.Add(CreateSistemLogModel(item));
            }
            return models;
        }

        public async Task<int> GetSistemLogsCountAsync(DataRequest<SistemLog> request)
        {
            return await _logRepository.GetLogsCountAsync(request);
        }

        public async Task SistemLogMarkAllAsReadAsync()
        {
            await _logRepository.MarkAllAsReadAsync();
        }

        public async Task<int> CreateLogAsync(SistemLog sistemLog) { return await _logRepository.CreateLogAsync(sistemLog); }
        public async Task WriteAsync(LogType type, string source, string action, string message, string description)
        {
            var sistemLog = new SistemLog()
            {
                User = _authenticationService.CurrentUsername,
                KaydedenId = _authenticationService.CurrentUserId,
                Type = type,
                Source = source,
                Action = action,
                Message = message,
                Description = description,
            };
            sistemLog.IsRead = type != LogType.Hata;
            await CreateLogAsync(sistemLog);
            _messageService.Send(this, "LogAdded", sistemLog);
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
        private SistemLogModel CreateSistemLogModel(SistemLog source)
        {
            return new SistemLogModel()
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
