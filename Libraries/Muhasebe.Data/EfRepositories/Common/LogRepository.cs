using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.Abstract.Sistem.Authentication;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Enum;
using Muhasebe.Domain.Utilities.IDGenerator;

namespace Muhasebe.Data.EfRepositories.Common
{
    public class LogRepository : ILogRepository
    {
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;
        private readonly IAuthenticator _authenticator;

        public LogRepository(IUnitOfWork<AppSistemDbContext> unitOfWork, IAuthenticator authenticator)
        {
            _unitOfWork = unitOfWork;
            _authenticator = authenticator;
        }
        private async Task<int> CommitAsync()
        {
            return await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        private IGenericRepository<AppLog> GenericRepository => _unitOfWork.GetRepository<IGenericRepository<AppLog>>();


        public async Task<int> CreateLogAsync(AppLog appLog)
        {
            appLog.Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Sistem);
            appLog.KayitTarihi = DateTime.UtcNow;
            await GenericRepository.AddAsync(appLog);
            return await CommitAsync();
        }

        public async Task<int> DeleteLogAsync(long id)
        {
            var log = await GenericRepository.GetByIdAsync(id).ConfigureAwait(false);
            if (log == null) throw new KeyNotFoundException("Log bulunamadı");
            await GenericRepository.DeleteAsync(log);
            return await CommitAsync();
        }

        public async Task<int> DeleteLogRangeAsync(int index, int length, DataRequest<AppLog> request)
        {
            var logs = await GenericRepository.GetPagedAsync(index, length, request).ConfigureAwait(false);
            if (logs == null) throw new KeyNotFoundException("Log bulunamadı");
            await GenericRepository.DeleteRangeAsync(logs.ToArray());
            return await CommitAsync();
        }

        public async Task<AppLog> GetLogAsync(long id)
        {
            return await GenericRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task<IList<AppLog>> GetLogsAsync()
        {
            return await GenericRepository.GetAllAsync().ConfigureAwait(false);
        }

        public async Task<IList<AppLog>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request)
        {
            var logList = await GenericRepository.GetPagedAsync(skip, take, request).ConfigureAwait(false);
            return logList;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<AppLog> request)
        {
            return await GenericRepository.CountAsync(request);
        }

        public async Task MarkAllAsReadAsync()
        {
            var logs = await GenericRepository.FindAsync(a => !a.IsRead).ConfigureAwait(false);
            foreach (var log in logs)
            {
                log.IsRead = true;
            }
            await CommitAsync();
        }

        public async Task WriteAsync(LogType type, string source, string action, string message, string description)
        {
            var currentUser = _authenticator.CurrentAccount;
            var applog = new AppLog()
            {
                User = currentUser.KullaniciAdi ?? "Sistem",
                Type = type,
                Source = source,
                Action = action,
                Message = $"[Kullanıcı: {currentUser}] {message}",
                Description = description,
                KaydedenId = currentUser.KaydedenId,
            };
            applog.IsRead = type != LogType.Hata;
            await CreateLogAsync(applog).ConfigureAwait(false);
        }

        public async Task WriteAsync(LogType type, string source, string action, Exception ex)
        {
            var description = ex.ToString(); // Tüm inner exception'ları içerir
            await WriteAsync(LogType.Hata, source, action, ex.Message, description).ConfigureAwait(false);
        }
    }
}
