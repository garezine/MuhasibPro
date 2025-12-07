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
    public class MaliDonemService : IMaliDonemService
    {
        private readonly IMaliDonemRepository _maliDonemRepository;
        private readonly ILogService _logService;
        private readonly IUnitOfWork<SistemDbContext> _unitOfWork;
        private readonly IAuthenticationService _authenticationService;
        private readonly IFirmaService _firmaService;
        private readonly IBitmapToolsService _bitmapToolsService;

        public MaliDonemService(
            IMaliDonemRepository maliDonemRepository,
            ILogService logService,
            IUnitOfWork<SistemDbContext> unitOfWork,
            IAuthenticationService authenticationService,
            IFirmaService firmaService,
            IBitmapToolsService bitmapToolsService)
        {
            _maliDonemRepository = maliDonemRepository;
            _logService = logService;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _firmaService = firmaService;
            _bitmapToolsService = bitmapToolsService;
        }
        public async Task<ApiDataResponse<MaliDonemModel>> GetByMaliDonemIdAsync(long id) 
        {
            try
            {
                var item = await GetByMaliDonemIdAsync(_maliDonemRepository,_bitmapToolsService, id);
                if (item.Data == null)
                    return new ErrorApiDataResponse<MaliDonemModel>(
                        data: null,
                        $"Firmaya ait mali dönem bulunamadı, ID: {id}");
                
                return new SuccessApiDataResponse<MaliDonemModel>(item.Data, "Firmaya ait Mali Donem veri işlemi başarılı");
            }
            catch (Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetByMaliDonemIdAsync), ex);
                return new ErrorApiDataResponse<MaliDonemModel>(data: null, message: ex.Message);
            }
        }
        private static async Task<ApiDataResponse<MaliDonemModel>> GetByMaliDonemIdAsync(IMaliDonemRepository repository,IBitmapToolsService bitmapTools, long id)
        {
            try
            {
                var item = await repository.GetByMaliDonemIdAsync(id);
                if(item == null)
                    return new ErrorApiDataResponse<MaliDonemModel>(
                        data: null,
                        $"Firmaya ait mali dönem bulunamadı, ID: {id}");
                var model = await CreateMaliDonemModelAsync(item, includeAllFields: true,bitmapTools);
                return new SuccessApiDataResponse<MaliDonemModel>(model, "Firmaya ait Mali Donem veri işlemi başarılı");
            } catch(Exception ex)
            {               
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
                var models = new List<MaliDonemModel>();
                var items = await _maliDonemRepository.GetMaliDonemlerAsync(skip, take, request);
                foreach(var item in items)
                {
                    models.Add(await CreateMaliDonemModelAsync(item, includeAllFields: false,_bitmapToolsService));
                }
                return new SuccessApiDataResponse<IList<MaliDonemModel>>(models, "Mali Donemler başarıyla listelendi.");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetMaliDonemlerAsync), ex);
                return new ErrorApiDataResponse<IList<MaliDonemModel>>(data: null, message: ex.Message);
            }
        }

        public async Task<ApiDataResponse<int>> UpdateMaliDonemAsync(MaliDonemModel model)
        {
            if(model == null)
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz!");
            long id = model.Id;
            try
            {
                var maliDonem = id > 0 ? await _maliDonemRepository.GetByMaliDonemIdAsync(id) : new MaliDonem();
                if(maliDonem == null)
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

                var result = await _unitOfWork.SaveChangesAsync();
                // Model'i güncel veriyle doldur
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemService),
                        nameof(UpdateMaliDonemAsync),
                        $"Mali Dömem başarıyla güncellendi. Firma ID: {id}",
                        $"Etkilenen kayıt: {result}");

                var updateMaliDonemModel = await GetByMaliDonemIdAsync(maliDonem.Id);
                if(updateMaliDonemModel.Success)
                {
                    model.Merge(updateMaliDonemModel.Data);
                }
                return new SuccessApiDataResponse<int>(data: result, message: "Mali Dönem başarıyla kaydedildi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(UpdateMaliDonemAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Kayıt hatası: {ex.Message}");
            }
        }

        public async Task<ApiDataResponse<int>> DeleteMaliDonemAsync(MaliDonemModel model)
        {
            if(model == null)
            {
                return new ErrorApiDataResponse<int>(data: 0, message: "Model boş olamaz");
            }
            try
            {
                var maliDonem = await _maliDonemRepository.GetByMaliDonemIdAsync(model.Id);
                if(maliDonem == null)
                {
                    return new ErrorApiDataResponse<int>(data: 0, message: "Mali Dönem bulunamadı");
                }
                await _maliDonemRepository.DeleteAsync(maliDonem);
                var result = await _unitOfWork.SaveChangesAsync();
                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemService),
                        nameof(DeleteMaliDonemAsync),
                        $"Mali dönem başarıyla silindi. Firma ID: {model.Id}",
                        $"Etkilenen kayıt: {result}");
                return new SuccessApiDataResponse<int>(result, "Mali Dönem başarıyla silindi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(DeleteMaliDonemAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }
        

        public async Task<ApiDataResponse<int>> DeleteMaliDonemRangeAsync(
            int index,
            int length,
            DataRequest<MaliDonem> request)
        {
            try
            {
                var items = await _maliDonemRepository.GetMaliDonemKeysAsync(index, length, request);
                if(items == null || !items.Any())
                {
                    return new ErrorApiDataResponse<int>(0, "Silinecek mali dönem bulunamadı");
                }

                await _maliDonemRepository.DeleteRangeAsync(items.ToArray());
                var result = await _unitOfWork.SaveChangesAsync();

                await _logService.SistemLogService
                    .SistemLogInformation(
                        nameof(MaliDonemService),
                        nameof(DeleteMaliDonemRangeAsync),
                        $"{items.Count} adet mali dönem başarıyla silindi. Index: {index}, Length: {length}",
                        $"Etkilenen kayıt: {result}");

                return new SuccessApiDataResponse<int>(result, $"{items.Count} adet firma başarıyla silindi");
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(DeleteMaliDonemRangeAsync), ex);
                return new ErrorApiDataResponse<int>(data: 0, message: $"Silme hatası: {ex.Message}");
            }
        }

        public async Task<int> GetMaliDonemlerCountAsync(DataRequest<MaliDonem> request)
        {
            try
            {
                return await _maliDonemRepository.GetMaliDonemlerCountAsync(request);
            } catch(Exception ex)
            {
                await _logService.SistemLogService
                    .SistemLogException(nameof(MaliDonemService), nameof(GetMaliDonemlerCountAsync), ex);
                return 0;
            }
        }

        public async Task<bool> IsMaliDonem(long firmaId, int maliYil)
        {
            var donem = await _maliDonemRepository.FindAsync(a => a.FirmaId == firmaId && a.MaliYil == maliYil);
            if(donem != null)
                return true;
            return false;
        }

        public static async Task<MaliDonemModel> CreateMaliDonemModelAsync(MaliDonem source, bool includeAllFields,IBitmapToolsService bitmapTools)
        {
            if(source == null)
                throw new ArgumentNullException("source");
            try
            {
                var model = new MaliDonemModel()
                {
                    Id = source.Id,
                    FirmaId = source.FirmaId,
                    MaliYil = source.MaliYil,
                    
                    DatabaseType = source.DatabaseType,
                    DBName = source.DBName,
                    DBPath = source.DBPath,
                    Directory = source.Directory,

                    //Base Entity
                    AktifMi = source.AktifMi,
                    KaydedenId = source.KaydedenId,
                    KayitTarihi = source.KayitTarihi,
                };
                if (source.Firma != null)
                {
                    model.FirmaModel = await FirmaService.CreateFirmaModelAsync(source.Firma, includeAllFields,bitmapTools);
                }
                return await Task.FromResult(model);
            } catch(Exception ex)
            {
                throw new Exception("MaliDonemModel oluşturulurken hata oluştu", ex);
            }
        }

        private void UpdateMaliDonemModel(MaliDonem target, MaliDonemModel source)
        {
            if(target == null)
                throw new ArgumentNullException(nameof(target));
            if(source == null)
                throw new ArgumentNullException(nameof(source));
            target.MaliYil = source.MaliYil;
            target.FirmaId = source.FirmaId;
            target.DatabaseType = source.DatabaseType;
            target.DBName = source.DBName;
            target.DBPath = source.DBPath;
            target.Directory = source.Directory;            
            
            //Base fields
            target.AktifMi = source.AktifMi;
            target.GuncellemeTarihi = source.GuncellemeTarihi;
            target.GuncelleyenId = source.GuncelleyenId;
        }

        public async Task<ApiDataResponse<MaliDonemModel>> CreateNewMaliDonemAsync(long firmaId)
        {
            var model = new MaliDonemModel { FirmaId = firmaId, MaliYil = DateTime.Now.Year };
            if(firmaId > 0)
            {
                var parent = await _firmaService.GetByFirmaIdAsync(firmaId);
                if(parent == null)
                {
                    await _logService.SistemLogService
                        .SistemLogError(
                            nameof(MaliDonemService),
                            nameof(CreateNewMaliDonemAsync),
                            $"Mali Dönem bulunamadı. Model ID: {model.FirmaId}");
                    return new ErrorApiDataResponse<MaliDonemModel>(data: null, message: "Mali Dönem bulunamadı");
                } else if(parent != null)
                {
                    model.FirmaId = firmaId;
                    model.FirmaModel = parent.Data;
                }
            }
            return new SuccessApiDataResponse<MaliDonemModel>(data: model, message: "Firma Mali Dönem'e entegre edildi");
        }

        public async Task<ApiDataResponse<IList<MaliDonemModel>>> GetMaliDonemlerAsync(DataRequest<MaliDonem> request)
        {
            return await GetMaliDonemlerAsync(0, 100, request);
        }
    }
}
