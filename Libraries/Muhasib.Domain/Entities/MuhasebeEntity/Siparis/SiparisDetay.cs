using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Stok;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Siparis
{
    [Table("SiparisDetay")]
    public class SiparisDetay : BaseEntity
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

        [ForeignKey("Siparisler")]
        public long SiparisId { get; set; }

        public bool SiparisTuru { get; set; }

        [MaxLength(255)]
        public string StokAdi { get; set; }

        public string StokAdiDetayli { get; set; }

        public short StokDusum { get; set; }

        [MaxLength(100)]
        public string StokKodu { get; set; }

        public long StokId { get; set; }

        public DateTime Tarih { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }

        public Siparisler Siparisler { get; set; }

        public ICollection<Stoklar> StokKartlar { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}