using Muhasib.Domain.Entities.MuhasebeEntity.Stok;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye
{

    public class FaturaKalemler : BaseEntity
    {
        [DatabaseGenerat‌​ed(DatabaseGeneratedOption.None)]
        public long FaturaId { get; set; }

        [DatabaseGenerat‌​ed(DatabaseGeneratedOption.None)]
        public int FaturaLine { get; set; }

        public long StokId { get; set; }

        public string StokKodu { get; set; }

        public string StokAdi { get; set; }

        public long BirimId { get; set; }

        public float KDVOran { get; set; }

        public bool KDVDurum { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Miktar { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal BirimFiyati { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutar { get; set; }

        public float IskontoOran { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal IskontoTutar { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal ToplamTutar { get; set; }

        public Faturalar Fatura { get; set; }

        public Stoklar Stok { get; set; }
    }
}
