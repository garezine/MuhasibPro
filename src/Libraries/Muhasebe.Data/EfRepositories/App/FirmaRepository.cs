using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.EfRepositories.Common;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Interfaces.App.IFirma;
using Muhasebe.Domain.Utilities.IDGenerator;

namespace Muhasebe.Data.EfRepositories.App
{
    public class FirmaRepository : GenericRepository<Firma>, IFirmaRepository
    {
        private readonly AppSistemDbContext _appSistemDbContext;
        public FirmaRepository(AppSistemDbContext context) : base(context) { _appSistemDbContext = context; }
        public async Task<Firma> GetByFirmaId(long id) { return await _appSistemDbContext.Firmalar.FindAsync(id); }

        public async Task<IList<Firma>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request)
        {
            IQueryable<Firma> items = GetQuery(request);
            var records = await items.Skip(skip)
                .Take(take)
                .Select(
                    r => new Firma
                    {
                        Adres = r.Adres,
                        AktifMi = r.AktifMi,
                        Eposta = r.Eposta,
                        GuncellemeTarihi = r.GuncellemeTarihi,
                        GuncelleyenId = r.GuncelleyenId,
                        Id = r.Id,
                        Il = r.Il,
                        Ilce = r.Ilce,
                        KaydedenId = r.KaydedenId,
                        KayitTarihi = r.KayitTarihi,
                        KisaUnvani = r.KisaUnvani,
                        LogoOnizleme = r.LogoOnizleme,
                        PBu1 = r.PBu1,
                        PBu2 = r.PBu2,
                        PostaKodu = r.PostaKodu,
                        TamUnvani = r.TamUnvani,
                        TCNo = r.TCNo,
                        Telefon1 = r.Telefon1,
                        Telefon2 = r.Telefon2,
                        VergiDairesi = r.VergiDairesi,
                        VergiNo = r.VergiNo,
                        Web = r.Web,
                        YetkiliKisi = r.YetkiliKisi,
                    })
                .AsNoTracking()
                .ToListAsync();
            return records;
        }

        public async Task<IList<Firma>> GetFirmaKeysAsync(int skip, int take, DataRequest<Firma> request)
        {
            IQueryable<Firma> items = GetQuery(request);
            var records = await items.Skip(skip)
                .Take(take)
                .Select(r => new Firma { Id = r.Id, })
                .AsNoTracking()
                .ToListAsync();
            return records;
        }

        public async Task<int> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            IQueryable<Firma> items = base.GetQuery(request);
            if(!string.IsNullOrEmpty(request.Query))
            {
                items.Where(r => r.SearchTerms.Contains(request.Query));
            }
            // Where
            if(request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.CountAsync();
        }

        public async Task UpdateFirmaAsync(Firma firma)
        {
            if (firma.Id > 0) 
            {
                await UpdateAsync(firma);
                firma.GuncellemeTarihi = DateTime.UtcNow;
            }
            else
            {
                firma.Id = UIDModuleGenerator.GenerateModuleId(UIDModuleType.Sistem);
                firma.KayitTarihi=DateTime.UtcNow;
                await AddAsync(firma);
            }            
            firma.SearchTerms = firma.BuildSearchTerms();
        }
        public async Task DeleteFirmalarAsync(params Firma[] firmalar) { await base.DeleteRangeAsync(firmalar); }

        public async Task<bool> IsFirma() { return await _appSistemDbContext.Firmalar.AnyAsync(); }
    }
}
