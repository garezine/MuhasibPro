using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Fatura_Irsaliye;
using Muhasebe.Domain.Entities.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Cari
{
    [Table("CariHareketler")]
    public class CariHareketler : BaseEntity
    {
        [Required] public long CariId { get; set; }

        [Required] public DateTime IslemTarihi { get; set; }

        [Required]
        [MaxLength(50)]
        public string IslemTipi { get; set; }//Ödeme-Tahsilat-Virman

        [MaxLength(50)]
        public string HesapSekli { get; set; }

        [MaxLength(50)]
        public string BelgeNo { get; set; }

        [MaxLength(150)]
        public string Aciklama { get; set; }

        [Required] public bool GC { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal IslemTutari { get; set; }

        public long? FaturaId { get; set; }

        public long? KasaId { get; set; }

        public CariHesap Cari { get; set; }

        public Faturalar Fatura { get; set; }

        public Kasalar Kasa { get; set; }
    }
}