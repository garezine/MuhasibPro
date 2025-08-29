using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.AppDb.Cari;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Kasa
{
    [Table("KasaHareket")]
    public class KasaHareket : BaseEntity
    {
        public long CariId { get; set; }

        public DateTime IslemTarihi { get; set; }

        [MaxLength(50)]
        public string IslemTipi { get; set; }

        [MaxLength(50)]
        public string HesapSekli { get; set; }

        [MaxLength(50)]
        public string BelgeNo { get; set; }

        [MaxLength(150)]
        public string Aciklama { get; set; }

        public bool GC { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutar { get; set; }

        public long KasaId { get; set; }
        public Kasalar Kasa { get; set; }
        public ICollection<CariHesap> CariHesaplar { get; set; }
    }
}