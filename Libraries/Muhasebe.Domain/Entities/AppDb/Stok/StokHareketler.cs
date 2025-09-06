using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.AppDb.Fatura_Irsaliye;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Stok
{
    [Table("StokHareketler")]
    public class StokHareketler : BaseEntity
    {
        [Required]
        public long StokId { get; set; }

        [Required]
        public DateTime IslemTarihi { get; set; }

        [MaxLength(50)]
        [Required]
        public string IslemTipi { get; set; }//Stok Giriþ Çýkýþ

        [MaxLength(50)]
        public string HesapSekli { get; set; }

        [MaxLength(50)]
        public string BelgeNo { get; set; }

        [MaxLength(150)]
        public string Aciklama { get; set; }
        [Required]
        public bool GC { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal BirimFiyat { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Miktar { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal ToplamFiyat { get; set; }
        public long? FaturaId { get; set; }
        public Stoklar Stok { get; set; }
        public Faturalar Fatura { get; set; }
    }
}
