using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.UIDGenerator;
using Muhasib.Domain.Entities.SistemEntity;
using System.Linq;

namespace Muhasib.Data.Repositories.SistemRepositories
{
    public class SistemLogRepository : BaseRepository<SistemDbContext, SistemLog>, ISistemLogRepository
    {
        public SistemLogRepository(SistemDbContext context) : base(context)
        {
        }
        public async Task<int> CreateLogAsync(SistemLog appLog)
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
        public async Task<int> DeleteLogsAsync(params SistemLog[] logs)
        {
            DbSet.RemoveRange(logs);
            return await Context.SaveChangesAsync();
        }
        public async Task<SistemLog> GetLogAsync(long id)
        {
            return await DbSet.FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<IList<SistemLog>> GetLogKeysAsync(int skip, int take, DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = GetLogs(request);
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
            IQueryable<SistemLog> items = DbSet;

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
        private IQueryable<SistemLog> GetLogs(DataRequest<SistemLog> request)
        {
            IQueryable<SistemLog> items = DbSet;

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
