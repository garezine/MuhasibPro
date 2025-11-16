using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Stok
{
    [Table("Stoklar")]
    public class Stoklar : BaseEntity
    {
        [Required]
        [MaxLength(50)] public string StokKodu { get; set; }

        [Required]
        public long StokGrupId { get; set; }

        [Required]
        [MaxLength(100)] public string StokAdi { get; set; }

        [Required]
        public long StokBirimId { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? AlisFiyati { get; set; }

        public bool? AlisKDVDurumu { get; set; }

        public float? KDVOrani { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? SatisFiyati { get; set; }

        public bool? SatisKDVDurumu { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? AcilisMiktar { get; set; }

        public bool? SatistaMi { get; set; }

        public long? BarkodId { get; set; }
        public byte[] Resim { get; set; }
        public byte[] ResimOnizle { get; set; }

        public Barkod Barkod { get; set; }

        public StokGruplar StokGrup { get; set; }

        public StokBirimler StokBirim { get; set; }

        public StokBakiyeler StokBakiyeler { get; set; }
        public ICollection<StokHareketler> StokHareketler { get; set; }
    }
}