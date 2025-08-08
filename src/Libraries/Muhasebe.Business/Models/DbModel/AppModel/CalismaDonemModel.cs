using Muhasebe.Business.Common;

namespace Muhasebe.Business.Models.DbModel.AppModel;

public class CalismaDonemModel : ObservableObject
{
    public static CalismaDonemModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long FirmaId { get; set; }

    public int CalismaYilDonem { get; set; }

    public FirmaModel Firma { get; set; }
    public CalismaDonemDbModel CalismaDonemDb { get; set; }

    public bool IsNew => Id <= 0;

    public override void Merge(ObservableObject source)
    {
        if (source is CalismaDonemModel model)
            Merge(model);
    }

    public void Merge(CalismaDonemModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaId = source.FirmaId;
            CalismaYilDonem = source.CalismaYilDonem;
            Firma = source.Firma;

            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;            
            
        }
    }

    public override string ToString() { return IsEmpty ? "----" : $"{CalismaYilDonem} - {Firma?.KisaUnvani}"; }
}