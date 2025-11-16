using Muhasib.Business.Models.Common;

namespace Muhasib.Business.Models.SistemModel;

public class FirmaModel : ObservableObject
{
    public static FirmaModel CreateEmpty()
        => new() { Id = -1, IsEmpty = true };

    public string FirmaKodu { get; set; }
    public string KisaUnvani { get; set; }
    public string TamUnvani { get; set; }
    public string YetkiliKisi { get; set; }
    public string Il { get; set; }
    public string Ilce { get; set; }
    public string Adres { get; set; }
    public string PostaKodu { get; set; }
    public string Telefon1 { get; set; }
    public string Telefon2 { get; set; }
    public string VergiDairesi { get; set; }
    public string VergiNo { get; set; }
    public string TCNo { get; set; }
    public string Web { get; set; }
    public string Eposta { get; set; }
    public byte[] Logo { get; set; }
    public byte[] LogoOnizleme { get; set; }
    public object LogoSource { get; set; }
    public object LogoOnizlemeSource { get; set; }
    public string PBu1 { get; set; } = "TL";
    public string PBu2 { get; set; }


    public ICollection<MaliDonemModel> MaliDonemler { get; set; }

    //Model değişiklikleri
    public bool IsNew => Id <= 0;
    public string Initials
        => string.Format("{0} {1}", $"{KisaUnvani}"[0], $"{KisaUnvani}"[0]).Trim().ToUpper();

    public override void Merge(ObservableObject source)
    {
        if (source is FirmaModel model)
            Merge(model);
    }

    public void Merge(FirmaModel source)
    {
        if (source != null)
        {
            Id = source.Id;
            FirmaKodu = source.FirmaKodu;
            KisaUnvani = source.KisaUnvani;
            TamUnvani = source.TamUnvani;
            YetkiliKisi = source.YetkiliKisi;
            Il = source.Il;
            Ilce = source.Ilce;
            Adres = source.Adres;
            PostaKodu = source.PostaKodu;
            Telefon1 = source.Telefon1;
            Telefon2 = source.Telefon2;
            VergiDairesi = source.VergiDairesi;
            VergiNo = source.VergiNo;
            TCNo = source.TCNo;
            Web = source.Web;
            Eposta = source.Eposta;
            Logo = source.Logo;
            LogoSource = source.LogoSource;
            LogoOnizleme = source.LogoOnizleme;
            LogoOnizlemeSource = source.LogoOnizlemeSource;
            PBu1 = source.PBu1;
            PBu2 = source.PBu2;

            AktifMi = source.AktifMi;
            KayitTarihi = source.KayitTarihi;
            GuncellemeTarihi = source.GuncellemeTarihi;
            KaydedenId = source.KaydedenId;
            GuncelleyenId = source.GuncelleyenId;

        }
    }

    public override string ToString()
    {
        return IsEmpty ? "----" : KisaUnvani;
    }
}