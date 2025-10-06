using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Business.Services.Abstracts.Common;
using Muhasebe.Business.Tools;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Data.Abstracts.Sistem;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemEntity;
using Muhasebe.Domain.Helpers;
using Muhasebe.Domain.Helpers.Responses;
using MuhasibPro.Collections.SistemCollection;
using MuhasibPro.Contracts.SistemServices;
using MuhasibPro.Extensions;
using MuhasibPro.Tools;

namespace MuhasibPro.Services.SistemServices.FirmaMaliDonemService
{
    public class FirmaService : IFirmaService
    {
        private readonly IFirmaRepository _firmaRepository;
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly IBitmapTools _bitmapTools;
        public ILogService LogService { get; private set; }

        public FirmaService(
            IFirmaRepository firmaRepository,
            ILogService logService,
            IUnitOfWork<AppSistemDbContext> unitOfWork,
            IAuthenticationService authenticationService,
            IBitmapTools bitmapTools)
        {
            _firmaRepository = firmaRepository ?? throw new ArgumentNullException(nameof(firmaRepository));
            LogService = logService ?? throw new ArgumentNullException(nameof(logService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _bitmapTools = bitmapTools ?? new BitmapTools();
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
                if (item == null)
                {
                    return new ErrorApiDataResponse<FirmaModel>(
                        data: null,
                        message: $"Firma bulunamadı. ID: {id}");
                }

                var model = await CreateFirmaModelAsync(item, includeAllFields: true);
                return new SuccessApiDataResponse<FirmaModel>(model, "Firma başarıyla getirildi");
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(GetFirmaAsync),
                    ex);
                return new ErrorApiDataResponse<FirmaModel>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<IList<FirmaModel>>> GetFirmalarAsync(int skip, int take, DataRequest<Firma> request)
        {
            try
            {
                var items = await _firmaRepository.GetFirmalarAsync(skip, take, request);
                var models = new List<FirmaModel>();

                foreach (var item in items)
                {
                    models.Add(await CreateFirmaModelAsync(item, includeAllFields: false));
                }

                return new SuccessApiDataResponse<IList<FirmaModel>>(models, "Firmalar başarıyla listelendi");
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(GetFirmalarAsync),
                    ex);
                return new ErrorApiDataResponse<IList<FirmaModel>>(data: null, message: ex.Message);
            }
        }

        public async Task<IList<FirmaModel>> GetFirmalarAsync(DataRequest<Firma> request)
        {
            var collection = new FirmaCollection(this, LogService);
            await collection.LoadAsync(request);
            return collection;
        }

        public Task<bool> IsFirma()
        {
            return _firmaRepository.IsFirma();
        }

        public async Task<ApiDataResponse<int>> UpdateFirmaAsync(FirmaModel model)
        {
            if (model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model null olamaz");
            }

            long id = model.Id;
            try
            {
                var firma = id > 0 ? await _firmaRepository.GetByFirmaId(id) : new Firma();
                if (firma == null)
                {
                    await LogService.SistemLogService.SistemLogError(
                        nameof(FirmaService),
                        nameof(UpdateFirmaAsync),
                        $"Firma güncellenemedi. Firma bulunamadı. Model ID: {id}");
                    return new ErrorApiDataResponse<int>(data: 0, message: "Firma bulunamadı");
                }

                firma.KaydedenId = _authenticationService.CurrentUserId;
                UpdateFirmaModel(firma, model);
                await _firmaRepository.UpdateFirmaAsync(firma);

                var result = await _unitOfWork.CommitAsync();

                // Transaction tamamlandıktan sonra log
                await LogService.SistemLogService.SistemLogInformation(
                    nameof(FirmaService),
                    nameof(UpdateFirmaAsync),
                    $"Firma başarıyla güncellendi. Firma ID: {id}",
                    $"Etkilenen kayıt: {result}");

                // Model'i güncel veriyle doldur
                var updateFirma = await GetFirmaAsync(firma.Id);
                if (updateFirma.Success)
                {
                    model.Merge(updateFirma.Data);
                }

                return new SuccessApiDataResponse<int>(result, "Firma başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(UpdateFirmaAsync),
                    ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Kayıt hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaAsync(FirmaModel model)
        {
            if (model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model null olamaz");
            }

            try
            {
                var firma = await _firmaRepository.GetByFirmaId(model.Id);
                if (firma == null)
                {
                    return new ErrorApiDataResponse<int>(data: 0, message: "Firma bulunamadı");
                }

                await _firmaRepository.DeleteAsync(firma);
                var result = await _unitOfWork.CommitAsync();

                await LogService.SistemLogService.SistemLogInformation(
                    nameof(FirmaService),
                    nameof(DeleteFirmaAsync),
                    $"Firma başarıyla silindi. Firma ID: {model.Id}",
                    $"Etkilenen kayıt: {result}");

                return new SuccessApiDataResponse<int>(result, "Firma başarıyla silindi");
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(DeleteFirmaAsync),
                    ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteFirmaRangeAsync(int index, int length, DataRequest<Firma> request)
        {
            try
            {
                var items = await _firmaRepository.GetFirmaKeysAsync(index, length, request);
                if (items == null || !items.Any())
                {
                    return new SuccessApiDataResponse<int>(0, "Silinecek firma bulunamadı");
                }

                await _firmaRepository.DeleteRangeAsync(items.ToArray());
                var result = await _unitOfWork.CommitAsync();

                await LogService.SistemLogService.SistemLogInformation(
                    nameof(FirmaService),
                    nameof(DeleteFirmaRangeAsync),
                    $"{items.Count} adet firma başarıyla silindi. Index: {index}, Length: {length}",
                    $"Etkilenen kayıt: {result}");

                return new SuccessApiDataResponse<int>(result, $"{items.Count} adet firma başarıyla silindi");
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(DeleteFirmaRangeAsync),
                    ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<int> GetFirmalarCountAsync(DataRequest<Firma> request)
        {
            try
            {
                return await _firmaRepository.GetFirmalarCountAsync(request);
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(GetFirmalarCountAsync),
                    ex);
                return 0;
            }
        }

        private async Task<FirmaModel> CreateFirmaModelAsync(Firma source, bool includeAllFields)
        {
            if (source == null)
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
                    AktifMi = source.AktifMi,
                    Eposta = source.Eposta,
                    GuncellemeTarihi = source.GuncellemeTarihi,
                    GuncelleyenId = source.GuncelleyenId,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                    KisaUnvani = source.KisaUnvani,
                    LogoOnizleme = source.LogoOnizleme,
                    LogoOnizlemeSource = await _bitmapTools.LoadBitmapAsync(source.LogoOnizleme),
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
                    model.LogoSource = await _bitmapTools.LoadBitmapAsync(source.Logo);
                    model.PBu1 = source.PBu1;
                    model.PBu2 = source.PBu2;
                    model.Telefon2 = source.Telefon2;
                    model.VergiDairesi = source.VergiDairesi;
                    model.VergiNo = source.VergiNo;
                    model.Web = source.Web;
                }

                return model; // Bu satır zaten var
            }
            catch (Exception ex)
            {
                await LogService.SistemLogService.SistemLogException(
                    nameof(FirmaService),
                    nameof(CreateFirmaModelAsync),
                    ex);
                throw new Exception("FirmaModel oluşturulurken hata oluştu", ex);
            }
        }

        private void UpdateFirmaModel(Firma target, FirmaModel source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            target.FirmaKodu = source.FirmaKodu;
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
    }
}