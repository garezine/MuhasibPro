using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using Muhasebe.Domain.Helpers.IDGenerator;

namespace Muhasebe.Data.Concrete.Common
{
    public class SistemLogRepository : GenericRepository<SistemLog>, ISistemLogRepository
    {
        private readonly AppSistemDbContext _dbContext;
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;

        public SistemLogRepository(IUnitOfWork<AppSistemDbContext> unitOfWork) : base(unitOfWork.Context)
        {
            _unitOfWork = unitOfWork;
            _dbContext = unitOfWork.Context ?? throw new ArgumentNullException(nameof(unitOfWork.Context));
        }

        public async Task<int> CreateLogAsync(SistemLog appLog)
        {
            appLog.Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Sistem);
            appLog.KayitTarihi = DateTime.UtcNow;
            await base.AddAsync(appLog);
            return await CommitAsync().ConfigureAwait(false);
        }

        public Task<int> DeleteLogsAsync(params SistemLog[] logs)
        {
            _dbContext.SistemLogs.RemoveRange(logs);
            return CommitAsync();
        }

        public async Task<SistemLog> GetLogAsync(long id)
        {
            return await _dbContext.FindAsync<SistemLog>(id);
        }

        public async Task<IList<SistemLog>> GetLogKeysAsync(int skip, int take, DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = GetLogs(request);

            // Execute
            var records = await items.Skip(skip).Take(take)
                .Select(r => new SistemLog
                {
                    Id = r.Id,
                })
                .AsNoTracking()
                .ToListAsync();

            return records;
        }

        public async Task<IList<SistemLog>> GetLogsAsync(int skip, int take, DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = GetLogs(request);

            // Execute
            var records = await items.Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync();

            return records;
        }

        public async Task<int> GetLogsCountAsync(DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = _dbContext.SistemLogs;

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
            var items = await _dbContext.SistemLogs.Where(r => !r.IsRead).ToListAsync();
            foreach (var item in items)
            {
                item.IsRead = true;
            }
            await CommitAsync();
        }
        private IQueryable<SistemLog> GetLogs(DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = _dbContext.SistemLogs;

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
