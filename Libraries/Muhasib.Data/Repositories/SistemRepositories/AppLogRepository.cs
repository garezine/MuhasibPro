using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.UIDGenerator;
using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

namespace Muhasib.Data.Repositories.SistemRepositories
{
    public class AppLogRepository : BaseRepository<AppDbContext, AppLog>, IAppLogRepository
    {        
        public AppLogRepository(AppDbContext context) : base(context)
        {            
        }

        public async Task<int> CreateLogAsync(AppLog appLog)
        {
            if (appLog == null)
                throw new ArgumentNullException(nameof(appLog));
            else
            {
                appLog.Id = UIDGenerator.GenerateModuleId(UIDModuleType.Sistem);
                appLog.KayitTarihi = DateTime.UtcNow;
                if (appLog.KaydedenId < -1)
                    appLog.KaydedenId = 0000000800; // Default to "korkutomer" user
                Context.Entry(appLog).State = EntityState.Added;
                return await Context.SaveChangesAsync();
            }
        }

        public async Task<int> DeleteLogsAsync(params AppLog[] logs)
        {
            DbSet.RemoveRange(logs);
            return await Context.SaveChangesAsync();
        }
        public async Task<AppLog> GetLogAsync(long id)
        {
            return await DbSet.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IList<AppLog>> GetLogKeysAsync(int skip, int take, DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = GetLogs(request);
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
            IQueryable<AppLog> items = DbSet;

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
            var items = await DbSet.Where(r => !r.IsRead).ToListAsync();
            foreach (var item in items)
            {
                item.IsRead = true;
            }
            await Context.SaveChangesAsync();
        }
        private IQueryable<AppLog> GetLogs(DataRequest<AppLog> request)
        {
            IQueryable<AppLog> items = DbSet;

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
    }
}
