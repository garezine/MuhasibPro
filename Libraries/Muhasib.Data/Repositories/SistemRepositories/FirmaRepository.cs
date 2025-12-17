using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities;
using Muhasib.Data.Utilities.UIDGenerator;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Repositories.SistemRepositories
{
    public class FirmaRepository : BaseRepository<SistemDbContext, Firma>, IFirmaRepository
    {
        public FirmaRepository(SistemDbContext context) : base(context)
        {
        }
        public async Task DeleteFirmalarAsync(params Firma[] firmalar)
        {
            await base.DeleteRangeAsync(firmalar);
        }
        #region FirmaKod Oluştur
        public async Task<string> GetYeniFirmaKodu(string customCode = null)
        {
            var request = new CodeGenerationRequest
            {
                Prefix = "F-",
                DigitLength = 4,
                AllowCustomCode = true,
                FillGaps = true,
                CaseSensitive = false
            };

            return await CodeGenerator.GenerateCodeAsync(
                getExistingCodes: GetOlusturulanFirmaKodListe,
                request: request,
                customCode: customCode
            );
        }
        private async Task<IEnumerable<string>> GetOlusturulanFirmaKodListe()
        {
            return await DbSet
                .Where(f => !string.IsNullOrWhiteSpace(f.FirmaKodu))
                .Select(f => f.FirmaKodu)
                .AsNoTracking()
                .ToListAsync();
        }
        #endregion

        public async Task<Firma> GetByFirmaId(long id)
        {
            return await DbSet.FindAsync(id).ConfigureAwait(false);
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
                        FirmaKodu = r.FirmaKodu,
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

        public async Task<int> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            IQueryable<Firma> items = GetQuery(request);
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

        public async Task<bool> IsFirma()
        {
            return await DbSet.AnyAsync().ConfigureAwait(false);
        }
        public async Task UpdateFirmaAsync(Firma firma)
        {
            if (firma.Id > 0)
            {
                firma.GuncellemeTarihi = DateTime.UtcNow;
                await UpdateAsync(firma);
            }
            else
            {
                firma.Id = UIDGenerator.GenerateModuleId(UIDModuleType.Sistem);
                firma.FirmaKodu = await GetYeniFirmaKodu();
                firma.KayitTarihi = DateTime.UtcNow;
                await AddAsync(firma);
            }
            firma.ArananTerim = firma.BuildSearchTerms();
        }


    }
}
