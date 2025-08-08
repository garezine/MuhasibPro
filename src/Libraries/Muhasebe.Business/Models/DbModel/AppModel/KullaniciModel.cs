using Muhasebe.Business.Common;

namespace Muhasebe.Business.Models.DbModel.AppModel;

public class KullaniciModel : ObservableObject
{
    public static KullaniciModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };
    public static readonly KullaniciModel Default = new()
    {
        KullaniciAdi = "korkutomer",
        Adi = "Ömer",
        Soyadi = "Korkut"
    };
    public string KullaniciAdi { get; set; }
    public string Adi { get; set; }
    public string Soyadi { get; set; }
    public string Eposta { get; set; }
    public string Rol { get; set; }
    public string Telefon { get; set; }
    public object PictureSource { get; set; }
    
    //BaseEntity    
    public string AdiSoyadi => $"{Adi} {Soyadi}";
    public string Initials => string.Format("{0}{1}", $"{Adi} "[0], $"{Soyadi} "[0]).Trim().ToUpper();

    public bool IsNew => Id <= 0;
    public ICollection<HesapModel> Hesaplar { get; set; }
    public override void Merge(ObservableObject source)
    {
        if (source is KullaniciModel model)
            Merge(model);
    }

    public void Merge(KullaniciModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            KullaniciAdi = source.KullaniciAdi;
            Adi = source.Adi;
            Soyadi = source.Soyadi;
            Eposta = source.Eposta;
            AktifMi = source.AktifMi;
            Rol = source.Rol;
            Telefon = source.Telefon;

            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;
            
            
        }
    }

    public override string ToString()
    {
        return IsEmpty ? "----" : AdiSoyadi;
    }
}