using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstract.App;
using Muhasebe.Business.Services.Abstract.Common;
using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.Abstract.Sistem;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Utilities.Responses;
using MuhasibPro.Core.Collections.App;
using MuhasibPro.Core.Infrastructure.Tools;

namespace MuhasibPro.Core.Services.Concreate.App
{
    public class FirmaService : IFirmaService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        public ILogService LogService { get; private set; }
        public FirmaService(IFirmaRepository firmaRepository, ILogService logService, IUnitOfWork<AppSistemDbContext> unitOfWork, IAuthenticationService authenticationService)
        {
            _firmaRepository = firmaRepository;
            LogService = logService;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }
        public async Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long id)
        {
            return await GetFirmaAsync(id).ConfigureAwait(false);
        }

        public async Task<ApiDataResponse<FirmaModel>> GetFirmaAsync(long id)
        {
            try
            {
                var item = await _firmaRepository.GetByFirmaId(id);
                if (item != null)
                {
                    var model = await CreateFirmaModelAsync(item, includeAllFields: true);
                    return new SuccessApiDataResponse<FirmaModel>(model, "Firma, FirmaModel'e dönüştürüldü");

                }
                return null;
            }
            catch (Exception ex)
            {
                return new ErrorApiDataResponse<FirmaModel>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request)
        {
            var models = new List<FirmaModel>();
            try
            {
                var items = await _firmaRepository.GetFirmalarAsync(skip, take, request);
                foreach (var item in items)
                {
                    models.Add(await CreateFirmaModelAsync(item, includeAllFields: false));
                }
                return new SuccessApiDataResponse<IList<FirmaModel>>(models, "Firmalar listelendi");
            }
            catch (Exception ex)
            {
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: null, message: ex.Message);
            }
        }
        public async Task<IList<FirmaModel>> GetFirmalarAsync(DataRequest<Firma> request)
        {
            var collection = new FirmaCollection(this, LogService);
            await collection.LoadAsync(request);
            return collection;
        }
        public Task<bool> IsFirma() { return _firmaRepository.IsFirma(); }

        public async Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model)
        {
            long id = model.Id;
            try
            {
                var firma = id > 0 ? await _firmaRepository.GetByFirmaId(id) : new Firma();
                if (firma != null)
                {
                    firma.KaydedenId = _authenticationService.CurrentUserId;
                    UpdateFirmaModelAsync(firma, model);
                    await _firmaRepository.UpdateFirmaAsync(firma);
                    var updateFirma = await GetFirmaAsync(firma.Id);
                    model.Merge(updateFirma.Data);

                    var result = await _unitOfWork.CommitAsync();
                    await LogService.LogInformationAsync(nameof(FirmaService), nameof(UpdateFirmaAsync),
                        $"Firma başarıyla güncellendi. Firma ID: {id}, Etkilenen kayıt: {result}");
                    return new SuccessApiDataResponse<int>(result, "Firma kaydedildi");
                }

                await LogService.LogErrorAsync(nameof(FirmaService), nameof(UpdateFirmaAsync),
                    $"Firma güncellenemedi. Firma bulunamadı. Model ID: {id}");
                return new ErrorApiDataResponse<int>(data: 0, "Firma kayıt hatası");
            }
            catch (Exception ex)
            {
                await LogService.LogExceptionAsync(nameof(FirmaService), nameof(UpdateFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, $"Kayıt hatası : {ex.Message}");
            }
        }
        public async Task<ApiDataResponse<int>> DeleteFirmaAsync(FirmaModel model)
        {
            try
            {
                var firma = await _firmaRepository.GetByFirmaId(model.Id);
                await _firmaRepository.DeleteAsync(firma);
                var result = await _unitOfWork.CommitAsync();

                await LogService.LogInformationAsync(nameof(FirmaService), nameof(DeleteFirmaAsync),
                    $"Firma başarıyla silindi. Firma ID: {model.Id}, Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, "Firma başarıyla silindi");
            }
            catch (Exception ex)
            {
                await LogService.LogExceptionAsync(nameof(FirmaService), nameof(DeleteFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, $"Silme hatası : {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request)
        {
            try
            {
                var items = await _firmaRepository.GetFirmaKeysAsync(index, length, request);
                await _firmaRepository.DeleteRangeAsync(items.ToArray());
                var result = await _unitOfWork.CommitAsync();

                await LogService.LogInformationAsync(nameof(FirmaService), nameof(DeleteFirmaRangeAsync),
                    $"{items.Count} adet firma başarıyla silindi. Index: {index}, Length: {length}, Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, "Seçili firma(lar) başarıyla silindi");
            }
            catch (Exception ex)
            {
                await LogService.LogExceptionAsync(nameof(FirmaService), nameof(DeleteFirmaRangeAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, $"Silme hatası : {ex.Message}");
            }
        }

        public static async Task<FirmaModel> CreateFirmaModelAsync(Firma source, bool includeAllFields)
        {
            try
            {
                var model = new FirmaModel()
                {
                    Id = source.Id,
                    Adres = source.Adres,
                    AktifMi = source.AktifMi,
                    Eposta = source.Eposta,
                    GuncellemeTarihi = source.GuncellemeTarihi,
                    GuncelleyenId = source.GuncelleyenId,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                    KisaUnvani = source.KisaUnvani,
                    LogoOnizleme = source.LogoOnizleme,
                    LogoOnizlemeSource = await BitmapTools.LoadBitmapAsync(source.LogoOnizleme),
                    PostaKodu = source.PostaKodu,
                    TamUnvani = source.TamUnvani,
                    TCNo = source.TCNo,
                    Telefon1 = source.Telefon1,
                    YetkiliKisi = source.YetkiliKisi,
                };

                if (includeAllFields)
                {
                    model.Il = source.Il;
                    model.Ilce = source.Ilce;
                    model.Logo = source.Logo;
                    model.LogoSource = await BitmapTools.LoadBitmapAsync(source.Logo);
                    model.PBu1 = source.PBu1;
                    model.PBu2 = source.PBu2;
                    model.Telefon2 = source.Telefon2;
                    model.VergiDairesi = source.VergiDairesi;
                    model.VergiNo = source.VergiNo;
                    model.Web = source.Web;
                }
                return model;
            }
            catch (Exception ex)
            {
                // Static method olduğu için log servisi kullanılamıyor
                // Hata durumunda exception fırlatılabilir veya null dönebilir
                throw new Exception("FirmaModel oluşturulurken hata oluştu", ex);
            }
        }
        private void UpdateFirmaModelAsync(Firma target, FirmaModel source)
        {
            target.Adres = source.Adres;
            target.AktifMi = source.AktifMi;
            target.Eposta = source.Eposta;
            target.GuncellemeTarihi = source.GuncellemeTarihi;
            target.GuncelleyenId = source.GuncelleyenId;

            target.KaydedenId = source.KaydedenId;
            target.KayitTarihi = source.KayitTarihi;
            target.KisaUnvani = source.KisaUnvani;
            target.LogoOnizleme = source.LogoOnizleme;
            target.PostaKodu = source.PostaKodu;
            target.TamUnvani = source.TamUnvani;
            target.TCNo = source.TCNo;
            target.Telefon1 = source.Telefon1;
            target.YetkiliKisi = source.YetkiliKisi;
            target.Il = source.Il;
            target.Ilce = source.Ilce;
            target.Logo = source.Logo;
            target.PBu1 = source.PBu1;
            target.PBu2 = source.PBu2;
            target.Telefon2 = source.Telefon2;
            target.VergiDairesi = source.VergiDairesi;
            target.VergiNo = source.VergiNo;
            target.Web = source.Web;
        }

        public async Task<int> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            try
            {
                return await _firmaRepository.GetFirmalarCountAsync(request);
            }
            catch (Exception ex)
            {
                var message = new ErrorApiDataResponse<int>(data: 0, message: ex.Message);
                return message.Data;
                throw;
            }
        }
    }
}
