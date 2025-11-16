using Muhasib.Business.Models.Common;

namespace Muhasib.Business.Models.SistemModel;

public class HesapModel : ObservableObject
{
    public static HesapModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public long KullaniciId { get; set; }
    public long FirmaId { get; set; }
    public long DonemId { get; set; }
    public KullaniciModel Kullanici { get; set; }
    public DateTime SonGirisTarihi { get; set; }
    public bool IsNew => Id <= 0;

    public override void Merge(ObservableObject source)
    {
        if (source is HesapModel model)
            Merge(model);
    }

    public void Merge(HesapModel model)
    {
        if (model != null)
        {
            Id = model.Id;
            KullaniciId = model.KullaniciId;
            FirmaId = model.FirmaId;
            DonemId = model.DonemId;
            Kullanici = model.Kullanici;
            SonGirisTarihi = model.SonGirisTarihi;

            KayitTarihi = model.KayitTarihi;
            GuncellemeTarihi = model.GuncellemeTarihi;
            KullaniciId = model.KullaniciId;
            GuncelleyenId = model.GuncelleyenId;

        }
    }

    public override string ToString()
    {
        return KullaniciId.ToString();
    }
}