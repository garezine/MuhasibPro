using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.TaksitOdemeTahsilat
{
    [Table("Taksitler")]
    public class Taksitler : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        public long CariId { get; set; }

        public long KasaHareketId { get; set; }

        public long KasaId { get; set; }

        public long TahsilHareketId { get; set; }

        [MaxLength(150)]
        public string CariUnvani { get; set; }

        [MaxLength(20)]
        public string Durumu { get; set; }

        [MaxLength(25)]
        public string IslemBelgeNo { get; set; }

        public DateTime OdemeTarihi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Odenen { get; set; }

        [MaxLength(10)]
        public string TaksitNo { get; set; }

        [MaxLength(15)]
        public string TaksitTipi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal TaksitTutari { get; set; }

        public bool Tipi { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }

        public ICollection<Kasalar> Kasalar { get; set; }

        public ICollection<GecikenTaksitler> GecikenTaksitler { get; set; }

        public ICollection<TaksitliAlis> TaksitliAlislar { get; set; }

        public ICollection<TaksitliSatis> TaksitliSatislar { get; set; }

        public ICollection<TaksitSenet> TaksitSenetler { get; set; }
    }
}
