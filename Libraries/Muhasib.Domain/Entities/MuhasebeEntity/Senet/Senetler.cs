using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Senet
{
    [Table("Senetler")]
    public class Senetler : BaseEntity
    {
        public string Aciklama { get; set; }

        [MaxLength(255)]
        public string AlacakliUnvani { get; set; }

        [MaxLength(255)]
        public string BorcluAdres1 { get; set; }

        [MaxLength(255)]
        public string BorcluAdres2 { get; set; }

        [MaxLength(255)]
        public string BorcluIsim { get; set; }

        [MaxLength(11)]
        public int BorcluTCKimlikNo { get; set; }

        [MaxLength(50)]
        public string BorcluVDN { get; set; }

        public long CariId { get; set; }

        [MaxLength(25)]
        public string CariyeIsleme { get; set; }

        [MaxLength(25)]
        public string Durumu { get; set; }

        public long CariHareketId { get; set; }

        [MaxLength(50)]
        public string IhtilafMahkemesi { get; set; }

        public DateTime SenetKayitTarihi { get; set; }

        [MaxLength(255)]
        public string Kefil { get; set; }

        [MaxLength(255)]
        public string KefilAdres { get; set; }

        [MaxLength(11)]
        public int KefilTCKimlikNo { get; set; }

        [MaxLength(50)]
        public string KefilVDN { get; set; }

        public long KasaId { get; set; }

        [MaxLength(20)]
        public string SenetNo { get; set; }

        [MaxLength(15)]
        public string SenetTuru { get; set; }

        public bool Turu { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }

        public DateTime VadeTarihi { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }

        public ICollection<Kasalar> Kasalar { get; set; }

        public ICollection<SenetTahsilatlari> SenetTahsilatlari { get; set; }

        public ICollection<SenetOdemeleri> SenetOdemeleri { get; set; }

        public ICollection<SenetMahkemeler> SenetMahkemeler { get; set; }

        public ICollection<SenetCirolari> SenetCirolar { get; set; }
    }
}
