using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

[Table("Ayarlar")]
public class Ayarlar
{
    [MaxLength(50)] public string Alan1 { get; set; }

    [MaxLength(50)] public string Alan2 { get; set; }

    [MaxLength(50)] public string Alan3 { get; set; }

    [MaxLength(50)] public string Alan4 { get; set; }

    [MaxLength(50)] public string Alan5 { get; set; }

    public bool CalismayanPersonelListesi { get; set; }

    [MaxLength(50)] public string CariFaturaTipi { get; set; }

    public bool CekCariyeIsleme { get; set; }

    public int DbBakimGunSay { get; set; }

    public DateTime DbBakimSonTarih { get; set; }

    public bool DbBakimUygula { get; set; }

    public decimal HatirlatmaAltLimit { get; set; }

    public bool HatirlatmaLimitKullan { get; set; }

    public decimal HatirlatmaUstLimit { get; set; }

    public bool IcindeArama { get; set; }

    public bool LK { get; set; }

    public bool Otomatikyedekleme { get; set; }

    public bool SenetCariyeisleme { get; set; }
    [MaxLength(50)] public string SenetIhtilafMahkemesi { get; set; }

    public string SonYedekKlasoru { get; set; }

    public bool StokBakiyeKontrolu { get; set; }

    public int StokKartiKDVOrani { get; set; }

    public bool YaziciListesi { get; set; }

    public string YedeklemeDizini { get; set; }
}
