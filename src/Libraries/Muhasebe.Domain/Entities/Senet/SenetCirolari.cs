using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Cari;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Senet
{
    [Table("SenetCirolari")]
    public class SenetCirolari : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        public long CariId { get; set; }

        public long CariHareketId { get; set; }

        [ForeignKey("Senetler")]
        public long SenetId { get; set; }

        public DateTime Tarih { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }
        public Senetler Senetler { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}