using Muhasib.Business.Models.Common;

namespace Muhasib.Business.Models.SistemModel;

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
    public long RolId { get; set; }
    public string Telefon { get; set; }
    public byte[]? Resim { get; set; }
    public byte[]? ResimOnizleme { get; set; }
    public object ResimSource { get; set; }
    public object ResimOnizlemeSource { get; set; }
    //BaseEntity    
    public string AdiSoyadi => $"{Adi} {Soyadi}";
    public string Initials => string.Format("{0}{1}", $"{Adi} "[0], $"{Soyadi} "[0]).Trim().ToUpper();

    public bool IsNew => Id <= 0;
    public ICollection<HesapModel> Hesaplar { get; set; }
    public KullaniciRolModel Rol { get; set; }
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
            RolId = source.RolId;
            Telefon = source.Telefon;

            AktifMi = source.AktifMi;
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