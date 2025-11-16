using Muhasib.Business.Infrastructure.Extensions;
using Muhasib.Business.Models.SistemModel;
using Muhasib.Business.Services.Contracts.BaseServices;
using Muhasib.Business.Services.Contracts.LogServices;
using Muhasib.Business.Services.Contracts.SistemServices;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Data.Utilities.Responses;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Business.Services.Concrete.SitemServices
{
    public class MaliDonemService : IMaliDonemService
    {
        private readonly IMaliDonemRepository _maliDonemRepository;
        private readonly ILogService _logService;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        public MaliDonemService(
            IMaliDonemRepository maliDonemRepository,
            ILogService logService,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _maliDonemRepository = maliDonemRepository;
            _logService = logService;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<MaliDonemModel>> GetByMaliDonemIdAsync(long id)
        {
            try
            {
                var item = await _maliDonemRepository.GetByMaliDonemId(id);
                if (item == null)
                    return new ErrorApiDataResponse<MaliDonemModel>(
                        data: null,
                        $"Firmaya ait mali dönem bulunamadı, ID: {id}");
                var model = await CreateMaliDonemModelAsync(item, includeAllFields: true);
                return new SuccessApiDataResponse<MaliDonemModel>(model, "Firmaya ait Mali Donem veri işlemi başarılı");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetByMaliDonemIdAsync), ex);
                return new ErrorApiDataResponse<MaliDonemModel>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<IList<MaliDonemModel>>> GetMaliDonemlerAsync(
            int skip,
            int take,
            DataRequest<MaliDonem> request)
        {
            try
            {
                var items = await _maliDonemRepository.GetMaliDonemlerAsync(skip, take, request);
                var models = new List<MaliDonemModel>();
                foreach (var item in items)
                {
                    models.Add(await CreateMaliDonemModelAsync(item, includeAllFields: false));
                }
                return new SuccessApiDataResponse<IList<MaliDonemModel>>(models, "Mali Donemler başarıyla listelendi.");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetMaliDonemlerAsync), ex);
                return new ErrorApiDataResponse<IList<MaliDonemModel>>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<int>> UpdateMaliDonemAsync(MaliDonemModel model)
        {
            if (model == null)
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz!");
            long id = model.Id;
            try
            {
                var maliDonem = id > 0
                    ? await _maliDonemRepository.GetByMaliDonemId(id)
                    : new MaliDonem();
                if (maliDonem == null)
                {
                    await _logService.SistemLogService
                        .SistemLogError(
                            nameof(MaliDonemService),
                            nameof(UpdateMaliDonemAsync),
                            $"Mali Dönem bulunamadı. Model ID: {model.FirmaId}");
                    return new ErrorApiDataResponse<int>(data: 0, message: "Mali Dönem bulunamadı");
                }
                maliDonem.KaydedenId = _authenticationService.CurrentUserId;
                UpdateMaliDonemModel(maliDonem, model);
                await _maliDonemRepository.UpdateMaliDonemAsync(maliDonem);

                var result = await _unitOfWork.CommitAsync();
                // Model'i güncel veriyle doldur
                await _logService.SistemLogService
                   .SistemLogInformation(
                       nameof(MaliDonemService),
                       nameof(UpdateMaliDonemAsync),
                       $"Mali Dömem başarıyla güncellendi. Firma ID: {id}",
                       $"Etkilenen kayıt: {result}");

                var updateMaliDonemModel = await GetByMaliDonemIdAsync(maliDonem.Id);
                if (updateMaliDonemModel.Success)
                {
                    model.Merge(updateMaliDonemModel.Data);
                }
                return new SuccessApiDataResponse<int>(data: result, message: "Mali Dönem başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(UpdateMaliDonemAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Kayıt hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteMaliDonemAsync(MaliDonemModel model)
        {
            if (model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            }
            try
            {
                var maliDonem = await _maliDonemRepository.GetByMaliDonemId(model.Id);
                if (maliDonem == null)
                {
                    return new ErrorApiDataResponse<int>(data: 0, message: "Mali Dönem bulunamadı");
                }
                await _maliDonemRepository.DeleteAsync(maliDonem);
                var result = await _unitOfWork.CommitAsync();
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemService),
                        nameof(DeleteMaliDonemAsync),
                        $"Firma başarıyla silindi. Firma ID: {model.Id}",
                        $"Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, "Mali Dönem başarıyla silindi");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(DeleteMaliDonemAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<int> GetMaliDonemlerCountAsync(DataRequest<MaliDonem> request)
        {
            try
            {
                return await _maliDonemRepository.GetMaliDonemlerCountAsync(request);
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetMaliDonemlerCountAsync), ex);
                return 0;
            }
        }

        public async Task<bool> IsMaliDonem(long firmaId)
        {
            var donem = await _maliDonemRepository.FindAsync(a => a.FirmaId == firmaId);
            if (donem != null)
                return true;
            return false;
        }

        public static async Task<MaliDonemModel> CreateMaliDonemModelAsync(MaliDonem source, bool includeAllFields)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            try
            {
                var model = new MaliDonemModel()
                {
                    Id = source.Id,
                    FirmaId = source.FirmaId,
                    MaliYil = source.MaliYil,
                    DbOlusturulduMu = source.DbOlusturulduMu,
                    //Base Entity
                    AktifMi = source.AktifMi,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                };
                if (source.Firma != null)
                {
                    model.FirmaModel = await FirmaService.CreateFirmaModelAsync(source.Firma, includeAllFields);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("MaliDonemModel oluşturulurken hata oluştu", ex);
            }
        }

        private void UpdateMaliDonemModel(MaliDonem target, MaliDonemModel source)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            target.MaliYil = source.MaliYil;
            target.DbOlusturulduMu = source.DbOlusturulduMu;
            //Base fields
            target.AktifMi = source.AktifMi;
            target.GuncellemeTarihi = source.GuncellemeTarihi;
            target.GuncelleyenId = source.GuncelleyenId;
        }
    }
}
