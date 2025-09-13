using Muhasebe.Business.Common;

namespace Muhasebe.Business.Models.SistemModel;

public class MaliDonemModel : ObservableObject
{
    public static MaliDonemModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long FirmaId { get; set; }

    public int MaliDonem { get; set; }

    public FirmaModel Firma { get; set; }
    public DonemDbSecModel DonemDbSec { get; set; }

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
            MaliDonem = source.MaliDonem;
            Firma = source.Firma;

            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;

        }
    }

    public override string ToString() { return IsEmpty ? "----" : $"{MaliDonem} - {Firma?.KisaUnvani}"; }
}