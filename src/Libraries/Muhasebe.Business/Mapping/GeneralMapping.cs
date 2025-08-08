using AutoMapper;
using Muhasebe.Business.Common;
using Muhasebe.Business.Models.DbModel.AppModel;
using Muhasebe.Business.Models.DbModel.Logs;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Sistem;
using Muhasebe.Domain.Entities.Uygulama;

namespace Muhasebe.Business.Mapping;

public class GeneralMapping : Profile
{
    public GeneralMapping()
    {
        //BaseEntity <->BaseModel için ortak kurallar
        CreateMap<BaseEntity, BaseModel>()
            .IncludeAllDerived()
            .ForMember(dest => dest.KayitTarihi, opt => opt.Ignore())
            .ForMember(dest => dest.KaydedenId, opt => opt.Ignore())
            .ReverseMap()
            .IncludeAllDerived()
            .ForMember(
                dest => dest.Id,
                opt =>
                {
                    opt.Condition((src, dest) => dest?.Id == 0); // Koşulu belirleyin
                    opt.MapFrom(src => src.Id); // MapFrom'u ekleyin
                })
            .ForMember(dest => dest.KayitTarihi, opt => opt.Ignore())
            .ForMember(dest => dest.KaydedenId, opt => opt.Ignore());

        CreateMap<AppLog, AppLogModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<Kullanici, KullaniciModel>()
            .ForMember(dest => dest.Hesaplar, opt => opt.MapFrom(src => src.Hesaplar))
            .IncludeBase<BaseEntity, BaseModel>()
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<Hesap, HesapModel>()
            .PreserveReferences()
            .ReverseMap();
            


        CreateMap<Firma, FirmaModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ForMember(dest => dest.CalismaDonemler, opt => opt.MapFrom(src => src.CalismaDonemler))
            
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<CalismaDonem, CalismaDonemModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ForMember(dest => dest.Firma, opt => opt.MapFrom(src => src.Firma))
            .ForMember(dest => dest.CalismaDonemDb, opt => opt.MapFrom(src => src.CalismaDonemDb))
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<CalismaDonemSec, CalismaDonemDbModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ForMember(dest => dest.CalismaDonem, opt => opt.MapFrom(src => src.CalismaDonem))
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();
    }
}