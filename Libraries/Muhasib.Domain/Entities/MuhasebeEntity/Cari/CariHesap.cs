using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariHesaplar")]
    public class CariHesap : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string CariKodu { get; set; }

        [Required]
        [MaxLength(100)]
        public string CariAdi { get; set; }

        [MaxLength(75)]
        public string YetkiliKisi { get; set; }

        [MaxLength(255)]
        public string Adres { get; set; }

        [MaxLength(17)]
        public string CepTel { get; set; }

        [MaxLength(17)]
        public string Faks { get; set; }

        public string Notlar { get; set; }

        [MaxLength(17)]
        public string Telefon1 { get; set; }

        [MaxLength(17)]
        public string Telefon2 { get; set; }

        [MaxLength(50)]
        public string EPosta { get; set; }

        [MaxLength(100)]
        public string WebSite { get; set; }

        public byte[] Resim { get; set; }

        public byte[] ResimOnizle { get; set; }

        public long CariGrupId { get; set; }

        public long CariBankaHesapId { get; set; }

        public CariGrup CariGrup { get; set; }

        public CariHesapDetay CariHesapDetay { get; set; }

        public CariFaturaBilgi CariFaturaBilgi { get; set; }

        public CariBakiyeler CariBakiyeler { get; set; }

        public ICollection<CariBankaHesap> CariBankaHesap { get; set; }

        public ICollection<CariHareketler> CariHareketler { get; set; }
    }
}