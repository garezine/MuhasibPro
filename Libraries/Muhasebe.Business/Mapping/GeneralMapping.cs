using AutoMapper;
using Muhasebe.Business.Common;
using Muhasebe.Business.Models.SistemModel;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.SistemEntity;

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

        CreateMap<SistemLog, SistemLogModel>()
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
            .ForMember(dest => dest.MaliDonemler, opt => opt.MapFrom(src => src.MaliDonemler))

            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<MaliDonem, MaliDonemModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ForMember(dest => dest.Firma, opt => opt.MapFrom(src => src.Firma))
            .ForMember(dest => dest.DonemDbSec, opt => opt.MapFrom(src => src.DonemDBSec))
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();

        CreateMap<DonemDBSec, DonemDbSecModel>()
            .IncludeBase<BaseEntity, BaseModel>()
            .ForMember(dest => dest.MaliDonem, opt => opt.MapFrom(src => src.MaliDonem))
            .PreserveReferences()
            .ReverseMap()
            .IncludeBase<BaseModel, BaseEntity>();
    }
}
