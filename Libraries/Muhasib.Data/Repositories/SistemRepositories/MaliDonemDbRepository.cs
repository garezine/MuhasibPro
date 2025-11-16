using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.UIDGenerator;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Repositories.SistemRepositories
{
    public class MaliDonemDbRepository : BaseRepository<SistemDbContext, MaliDonemDb>, IMaliDonemDbRepository
    {
        public MaliDonemDbRepository(SistemDbContext context) : base(context)
        {
        }

        public async Task DeleteMaliDonemDbAsync(params MaliDonemDb[] maliDonemDbler)
        {
            await base.DeleteRangeAsync(maliDonemDbler);
        }
        public MaliDonemDb GetByMaliDonemDbId(long maliDonemId)
        {
            return DbSet.Where(r => r.MaliDonemId == maliDonemId)
                .Include(r => r.MaliDonem)
                .FirstOrDefault();
        }
        public async Task<MaliDonemDb> GetByMaliDonemDbIdAsync(long maliDonemId)
        {
            return await DbSet.Where(r => r.MaliDonemId == maliDonemId)
                .Include(r => r.MaliDonem)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<IList<MaliDonemDb>> GetMaliDonemDbKeysAsync(int skip, int take, DataRequest<MaliDonemDb> request)
        {
            IQueryable<MaliDonemDb> items = GetQuery(request);
            var record = await items.Skip(skip).Take(take)
                .Select(r => new MaliDonemDb
                {
                    Id = r.Id,
                    MaliDonemId = r.MaliDonemId,
                })
                .AsNoTracking()
                .ToListAsync();
            return record;
        }

        public async Task<IList<MaliDonemDb>> GetMaliDonemDblerAsync(int skip, int take, DataRequest<MaliDonemDb> request)
        {
            IQueryable<MaliDonemDb> items = GetQuery(request);
            var records = await items.Skip(skip).Take(take)
                .Include(r => r.MaliDonem)
                .AsNoTracking().ToListAsync();
            return records;
        }

        public async Task<int> GetMaliDonemDblerCountAsync(DataRequest<MaliDonemDb> request)
        {
            IQueryable<MaliDonemDb> items = GetQuery(request);

            if (!string.IsNullOrEmpty(request.Query))
            {
                items.Where(r => r.ArananTerim.Contains(request.Query));
            }
            // Where
            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.CountAsync();
        }

        public async Task<bool> IsMaliDonemDb()
        {
            return await DbSet.AnyAsync();
        }

        public async Task UpdateMaliDonemDbAsync(MaliDonemDb maliDonemDb)
        {
            if (maliDonemDb.Id > 0)
            {
                maliDonemDb.GuncellemeTarihi = DateTime.UtcNow;
                await UpdateAsync(maliDonemDb);
            }
            else
            {
                if (maliDonemDb != null)
                {
                    maliDonemDb.Id = UIDGenerator.GenerateModuleId(UIDModuleType.Sistem);
                    maliDonemDb.KayitTarihi = DateTime.UtcNow;
                    await AddAsync(maliDonemDb);

                }
            }
            maliDonemDb.ArananTerim = maliDonemDb.BuildSearchTerms();
        }
    }
}
