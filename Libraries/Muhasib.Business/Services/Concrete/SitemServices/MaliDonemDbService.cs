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
    public class MaliDonemDbService : IMaliDonemDbService
    {
        private readonly IMaliDonemDbRepository _donemDbRepository;
        private readonly ILogService _logService;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        public MaliDonemDbService(
            IMaliDonemDbRepository donemDbRepository,
            ILogService logService,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _donemDbRepository = donemDbRepository;
            _logService = logService;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<ApiDataResponse<MaliDonemDbModel>> GetByMaliDonemDbIdAsync(long maliDonemId)
        {
            try
            {
                var item = await _donemDbRepository.GetByMaliDonemDbIdAsync(maliDonemId);
                if (item == null)
                    return new ErrorApiDataResponse<MaliDonemDbModel>(
                        data: null,
                        $"Döneme ait veritabanı bulunamadı, ID: {maliDonemId}");
                var model = await CreateMaliDonemDbModelAsync(item, includeAllFields: true);
                return new SuccessApiDataResponse<MaliDonemDbModel>(
                    model,
                    "Döneme ait veritabanı bilgilileri alma işlemi başarılı");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemDbService), nameof(GetByMaliDonemDbIdAsync), ex);
                return new ErrorApiDataResponse<MaliDonemDbModel>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<IList<MaliDonemDbModel>>> GetMaliDonemDblerAsync(
            int skip,
            int take,
            DataRequest<MaliDonemDb> request)
        {
            try
            {
                var items = await _donemDbRepository.GetMaliDonemDblerAsync(skip, take, request);
                var models = new List<MaliDonemDbModel>();
                foreach (var item in items)
                {
                    models.Add(await CreateMaliDonemDbModelAsync(item, includeAllFields: false));
                }
                return new SuccessApiDataResponse<IList<MaliDonemDbModel>>(
                    models,
                    "Döneme ait veritabanı başarıyla listelendi.");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemDbService), nameof(GetMaliDonemDblerAsync), ex);
                return new ErrorApiDataResponse<IList<MaliDonemDbModel>>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<int>> UpdateMaliDonemDbAsync(MaliDonemDbModel model)
        {
            if (model == null)
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            long id = model.MaliDonemId;
            try
            {
                var donemDb = id > 0
                    ? await _donemDbRepository.GetByMaliDonemDbIdAsync(model.MaliDonemId)
                    : new MaliDonemDb();

                if (donemDb == null)
                {
                    await _logService.SistemLogService
                        .SistemLogError(
                            nameof(MaliDonemDbService),
                            nameof(UpdateMaliDonemDbAsync),
                            $"Döneme ait veritabanı bilgileri bulunamadı. Model ID: {model.MaliDonemId}");
                    return new ErrorApiDataResponse<int>(data: 0, message: "Döneme ait veritabanı bilgileri bulunamadı");
                }
                donemDb.KaydedenId = _authenticationService.CurrentUserId;
                UpdateMaliDonemDbModel(donemDb, model);
                await _donemDbRepository.UpdateMaliDonemDbAsync(donemDb);

                var result = await _unitOfWork.CommitAsync();

                // Transaction tamamlandıktan sonra log
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemDbService),
                        nameof(UpdateMaliDonemDbAsync),
                        $"Döneme ait veritabanı bilgileri başarıyla güncellendi. Firma ID: {id}",
                        $"Etkilenen kayıt: {result}");

                //Modeli güncel veriyle doldur
                var updateDbModel = await GetByMaliDonemDbIdAsync(donemDb.MaliDonemId);
                if (updateDbModel.Success)
                    model.Merge(updateDbModel.Data);

                return new SuccessApiDataResponse<int>(
                    data: result,
                    message: "Döneme ait veritabanı bilgileri başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemDbService), nameof(UpdateMaliDonemDbAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Kayıt hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteMaliDonemDbAsync(MaliDonemDbModel model)
        {
            if (model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            }
            try
            {
                var donemDb = await _donemDbRepository.GetByMaliDonemDbIdAsync(model.MaliDonemId);
                if (donemDb == null)
                {
                    return new ErrorApiDataResponse<int>(data: 0, message: "Döneme ait veritabanı bilgileri bulunamadı");
                }
                await _donemDbRepository.DeleteAsync(donemDb);
                var result = await _unitOfWork.CommitAsync();
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemDbService),
                        nameof(DeleteMaliDonemDbAsync),
                        $"Döneme ait veritabanı bilgileri başarıyla silindi. Dönem ID: {model.MaliDonemId}",
                        $"Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, "Döneme ait veritabanı bilgileri başarıyla silindi");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemDbService), nameof(DeleteMaliDonemDbAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<int> GetMaliDonemDblerCountAsync(DataRequest<MaliDonemDb> request)
        {
            try
            {
                return await _donemDbRepository.GetMaliDonemDblerCountAsync(request);
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemDbService), nameof(GetMaliDonemDblerCountAsync), ex);
                return 0;
            }
        }

        public static async Task<MaliDonemDbModel> CreateMaliDonemDbModelAsync(
            MaliDonemDb source,
            bool includeAllFields)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            try
            {
                var model = new MaliDonemDbModel()
                {
                    Id = source.Id,
                    DatabaseType = source.DatabaseType,
                    DBName = source.DBName,
                    DBPath = source.DBPath,
                    Directory = source.Directory,
                    MaliDonemId = source.MaliDonemId,
                    //Base Entity
                    AktifMi = source.AktifMi,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                };
                if (source.MaliDonem != null)
                    model.MaliDonemModel = await MaliDonemService.CreateMaliDonemModelAsync(
                        source.MaliDonem,
                        includeAllFields);
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("MaliDonemModel oluşturulurken hata oluştu", ex);
            }
        }

        private void UpdateMaliDonemDbModel(MaliDonemDb target, MaliDonemDbModel source)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            target.MaliDonemId = source.MaliDonemId;
            target.DatabaseType = source.DatabaseType;
            target.DBName = source.DBName;
            target.DBPath = source.DBPath;
            target.Directory = source.Directory;
            //Base fields
            target.AktifMi = source.AktifMi;
            target.GuncellemeTarihi = source.GuncellemeTarihi;
            target.GuncelleyenId = source.GuncelleyenId;
        }
    }
}
