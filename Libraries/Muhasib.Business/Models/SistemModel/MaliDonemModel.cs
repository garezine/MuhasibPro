using Muhasib.Business.Models.Common;

namespace Muhasib.Business.Models.SistemModel;

public class MaliDonemModel : ObservableObject
{
    public static MaliDonemModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long FirmaId { get; set; }
    public int MaliYil { get; set; }
    public bool DbOlusturulduMu { get; set; }

    public FirmaModel FirmaModel { get; set; }
    public MaliDonemDbModel MaliDonemDbModel { get; set; }
    public bool IsNew => Id <= 0;

    public override void Merge(ObservableObject source)
    {
        if (source is MaliDonemModel model)
            Merge(model);
    }

    public void Merge(MaliDonemModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaId = source.FirmaId;
            MaliYil = source.MaliYil;
            DbOlusturulduMu = source.DbOlusturulduMu;
            FirmaModel = source.FirmaModel;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;

        }
    }

    public override string ToString() { return IsEmpty ? "----" : $"{MaliYil} - {FirmaModel?.KisaUnvani}"; }
}