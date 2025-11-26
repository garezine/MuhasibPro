using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Business.Services.Contracts.UtilityServices;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Concrete.SistemServices
{
    public class FirmaService : IFirmaService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IBitmapToolsService _bitmapTools;
        private readonly ILogService _logService;
        private readonly IAuthenticationService _authenticationService;

        public FirmaService(
            IFirmaRepository firmaRepository,
            ILogService logService,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IBitmapToolsService bitmapTools,
            IAuthenticationService authenticationService)
        {
            _firmaRepository = firmaRepository;
            _logService = logService;
            _unitOfWork = unitOfWork;
            _bitmapTools = bitmapTools;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<FirmaModel>> GetByFirmaIdAsync(long id)
        {
            try
            {
                var item = await _firmaRepository.GetByFirmaId(id);
                if(item == null)
                {
                    return new ErrorApiDataResponse<FirmaModel>(data: null, message: $"Firma bulunamadı. ID: {id}");
                }

                var model = await CreateFirmaModelAsync(item, includeAllFields: true, bitmapToolsService: _bitmapTools);
                return new SuccessApiDataResponse<FirmaModel>(model, "Firma verileri işlemi başarılı");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(GetByFirmaIdAsync), ex);
                return new ErrorApiDataResponse<FirmaModel>(data: null, message: ex.Message);
            }
        }


        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarAsync(
            int skip,
            int take,
            DataRequest<Firma> request)
        {
            try
            {
                var models = new List<FirmaModel>();
                var items = await _firmaRepository.GetFirmalarAsync(skip, take, request);

                foreach(var item in items)
                {
                    models.Add(
                        await CreateFirmaModelAsync(item, includeAllFields: false, bitmapToolsService: _bitmapTools));
                }

                return new SuccessApiDataResponse<IList<FirmaModel>>(models, "Firmalar başarıyla listelendi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(GetFirmalarAsync), ex);
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model)
        {
            if(model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            }

            long id = model.Id;
            try
            {
                var firma = id > 0 ? await _firmaRepository.GetByFirmaId(id) : new Firma();
                if(firma == null)
                {
                    await _logService.SistemLogService
                        .SistemLogError(
                            nameof(FirmaService),
                            nameof(UpdateFirmaAsync),
                            $"Firma güncellenemedi. Firma bulunamadı. Model ID: {id}");
                    return new ErrorApiDataResponse<int>(data: 0, message: "Firma bulunamadı");
                }
                if(id > 0)
                    firma.GuncelleyenId = _authenticationService.CurrentUserId;

                firma.KaydedenId = _authenticationService.CurrentUserId;

                UpdateFirmaModel(firma, model);
                await _firmaRepository.UpdateFirmaAsync(firma);

                var result = await _unitOfWork.SaveChangesAsync();

                // Transaction tamamlandıktan sonra log
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(FirmaService),
                        nameof(UpdateFirmaAsync),
                        $"Firma başarıyla güncellendi. Firma ID: {id}",
                        $"Etkilenen kayıt: {result}");

                // Model'i güncel veriyle doldur
                var updateFirma = await GetByFirmaIdAsync(firma.Id);
                if(updateFirma.Success)
                {
                    model.Merge(updateFirma.Data);
                }

                return new SuccessApiDataResponse<int>(result, "Firma başarıyla kaydedildi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(UpdateFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Kayıt hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaAsync(FirmaModel model)
        {
            if(model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            }

            try
            {
                var firma = await _firmaRepository.GetByFirmaId(model.Id);
                if(firma == null)
                {
                    return new ErrorApiDataResponse<int>(data: 0, message: "Firma bulunamadı");
                }

                await _firmaRepository.DeleteFirmalarAsync(firma);
                var result = await _unitOfWork.SaveChangesAsync();

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(FirmaService),
                        nameof(DeleteFirmaAsync),
                        $"Firma başarıyla silindi. Firma ID: {model.Id}",
                        $"Etkilenen kayıt: {result}");

                return new SuccessApiDataResponse<int>(result, "Firma başarıyla silindi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(DeleteFirmaAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request)
        {
            try
            {
                var items = await _firmaRepository.GetFirmaKeysAsync(index, length, request);
                if(items == null || !items.Any())
                {
                    return new SuccessApiDataResponse<int>(0, "Silinecek firma bulunamadı");
                }

                await _firmaRepository.DeleteRangeAsync(items.ToArray());
                var result = await _unitOfWork.SaveChangesAsync();

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(FirmaService),
                        nameof(DeleteFirmaRangeAsync),
                        $"{items.Count} adet firma başarıyla silindi. Index: {index}, Length: {length}",
                        $"Etkilenen kayıt: {result}");

                return new SuccessApiDataResponse<int>(result, $"{items.Count} adet firma başarıyla silindi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(DeleteFirmaRangeAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<int> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            try
            {
                return await _firmaRepository.GetFirmalarCountAsync(request);
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(FirmaService), nameof(GetFirmalarCountAsync), ex);
                return 0;
            }
        }

        public Task<bool> IsFirma() { return _firmaRepository.IsFirma(); }

        public static async Task<FirmaModel> CreateFirmaModelAsync(
            Firma source,
            bool includeAllFields,
            IBitmapToolsService bitmapToolsService = null)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                var model = new FirmaModel()
                {
                    Id = source.Id,
                    FirmaKodu = source.FirmaKodu,
                    Adres = source.Adres,
                    Eposta = source.Eposta,
                    KisaUnvani = source.KisaUnvani,
                    LogoOnizleme = source.LogoOnizleme,
                    LogoOnizlemeSource = await bitmapToolsService.LoadBitmapAsync(source.LogoOnizleme),
                    PostaKodu = source.PostaKodu,
                    TamUnvani = source.TamUnvani,
                    TCNo = source.TCNo,
                    Telefon1 = source.Telefon1,
                    YetkiliKisi = source.YetkiliKisi,

                    //Base Entity
                    AktifMi = source.AktifMi,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                    GuncelleyenId = source.GuncelleyenId,
                    GuncellemeTarihi = source.GuncellemeTarihi,
                };

                if(includeAllFields)
                {
                    model.Il = source.Il;
                    model.Ilce = source.Ilce;
                    model.Logo = source.Logo;
                    model.LogoSource = await bitmapToolsService.LoadBitmapAsync(source.Logo);
                    model.PBu1 = source.PBu1;
                    model.PBu2 = source.PBu2;
                    model.Telefon2 = source.Telefon2;
                    model.VergiDairesi = source.VergiDairesi;
                    model.VergiNo = source.VergiNo;
                    model.Web = source.Web;
                }

                return model; // Bu satır zaten var
            } catch(Exception ex)
            {
                throw new Exception("FirmaModel oluşturulurken hata oluştu", ex);
            }
        }

        private void UpdateFirmaModel(Firma target, FirmaModel source)
        {
            target.Adres = source.Adres;
            target.Eposta = source.Eposta;
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

            //Base fields
            target.AktifMi = source.AktifMi;
            target.KaydedenId = source.KaydedenId;
            target.GuncelleyenId = source.GuncelleyenId;
        }

        public async Task<string> GetYeniFirmaKodu() { return await _firmaRepository.GetYeniFirmaKodu(); }
    }
}