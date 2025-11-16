using Muhasib.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
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