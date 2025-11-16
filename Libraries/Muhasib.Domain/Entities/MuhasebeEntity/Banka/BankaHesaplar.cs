using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Banka
{
    [Table("BankaHesaplar")]
    public class BankaHesaplar : BaseEntity
    {
        [Required] public long BankaListId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HesapNo { get; set; }

        [Required]
        [MaxLength(255)]
        public string HesapSahibi { get; set; }

        [Required]
        [MaxLength(50)]
        public string HesapAdi { get; set; }

        [Required]
        [MaxLength(50)]
        public string SubeAdi { get; set; }

        [Required]
        [MaxLength(26)]
        public string IBAN { get; set; }

        public string Notlar { get; set; }

        [MaxLength(10)]
        [Required]
        public long ParaBirimId { get; set; }

        [MaxLength(17)]
        public string Telefon1 { get; set; }

        [MaxLength(17)]
        public string Telefon2 { get; set; }

        [MaxLength(20)]
        public string Faks { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Bakiye { get; set; } = 0;

        public ICollection<BankaHareket> BankaHareketler { get; set; }

        public BankaListesi BankaList { get; set; }

        public ParaBirimler ParaBirim { get; set; }
    }
}