using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Helpers;
using Muhasebe.Domain.Helpers.IDGenerator;

namespace Muhasebe.Data.Concrete.Common
{
    public class AppLogRepository : GenericRepository<AppLog>, IAppLogRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IUnitOfWork<AppDbContext> _unitOfWork;

        public AppLogRepository(IUnitOfWork<AppDbContext> unitOfWork) : base(unitOfWork.Context)
        {
            _unitOfWork = unitOfWork;
            _dbContext = unitOfWork.Context ?? throw new ArgumentNullException(nameof(unitOfWork.Context));
        }

        public async Task<int> CreateLogAsync(AppLog appLog)
        {
            appLog.Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Sistem);
            appLog.KayitTarihi = DateTime.UtcNow;
            await base.AddAsync(appLog);
            return await CommitAsync().ConfigureAwait(false);
        }

        public Task<int> DeleteLogsAsync(params AppLog[] logs)
        {
            base.DeleteRangeAsync(logs);
            return CommitAsync();
        }

        public async Task<AppLog> GetLogAsync(long id)
        {
            return await base.GetByIdAsync(id);
        }

        public async Task<IList<AppLog>> GetLogKeysAsync(int skip, int take, DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = GetLogs(request);

            // Execute
            var records = await items.Skip(skip).Take(take)
                .Select(r => new AppLog
                {
                    Id = r.Id,
                })
                .AsNoTracking()
                .ToListAsync();

            return records;
        }

        public async Task<IList<AppLog>> GetLogsAsync(int skip, int take, DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = GetLogs(request);

            // Execute
            var records = await items.Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync();

            return records;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = _dbContext.AppLogs;

            // Query
            if (!String.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.Message.Contains(request.Query.ToLower()));
            }

            // Where
            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.CountAsync();
        }

        public async Task MarkAllAsReadAsync()
        {
            var items = await _dbContext.AppLogs.Where(r => !r.IsRead).ToListAsync();
            foreach (var item in items)
            {
                item.IsRead = true;
            }
            await CommitAsync();
        }
        private IQueryable<AppLog> GetLogs(DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = _dbContext.AppLogs;

            // Query
            if (!String.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.Message.ToLower().Contains(request.Query.ToLower()));
            }

            // Where
            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            // Order By
            if (request.OrderBy != null)
            {
                items = items.OrderBy(request.OrderBy);
            }
            if (request.OrderByDesc != null)
            {
                items = items.OrderByDescending(request.OrderByDesc);
            }

            return items;
        }

        private async Task<int> CommitAsync()
        {
            return await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }

    }
}
