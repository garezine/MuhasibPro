using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Senet
{
    [Table("SenetOdemeleri")]
    public class SenetOdemeleri : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        public long KasaId { get; set; }

        [MaxLength(50)]
        public string OdemeSekli { get; set; }

        [ForeignKey("Senetler")]
        public long SenetId { get; set; }

        public DateTime Tarih { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }

        public ICollection<Kasalar> Kasalar { get; set; }

        public Senetler Senetler { get; set; }
    }
}