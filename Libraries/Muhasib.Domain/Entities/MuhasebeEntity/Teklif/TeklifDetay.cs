using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Stok;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Teklif
{
    [Table("TeklifDetaylar")]
    public class TeklifDetay : BaseEntity
    {
        [Column(TypeName = "decimal (18,2)")]
        public decimal BirimFiyati { get; set; }

        [MaxLength(50)]
        public string Birimi { get; set; }

        public long CariId { get; set; }

        public float IskontoOrani { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal IskontoTutari { get; set; }

        public float KDVOrani { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal KDVTutari { get; set; }

        public float Miktari { get; set; }

        [MaxLength(50)]
        public string StokAdi { get; set; }

        public string StokAdiDetayli { get; set; }

        public short StokDusum { get; set; }

        [MaxLength(50)]
        public string StokKodu { get; set; }

        public long StokId { get; set; }

        public DateTime Tarih { get; set; }

        public long TeklifId { get; set; }

        public bool TeklifTuru { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }

        public Teklifler Teklifler { get; set; }

        public ICollection<Stoklar> StokKartlar { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}