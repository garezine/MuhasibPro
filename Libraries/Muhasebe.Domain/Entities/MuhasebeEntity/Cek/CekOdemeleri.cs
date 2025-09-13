using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.MuhasebeEntity.Kasa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.MuhasebeEntity.Cek
{
    [Table("CekOdemeleri")]
    public class CekOdemeleri : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        [ForeignKey("Cekler")]
        public long CekId { get; set; }

        [ForeignKey("Kasalar")]
        public long KasaId { get; set; }

        [MaxLength(50)]
        public string OdemeSekli { get; set; }

        public DateTime Tarih { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Tutari { get; set; }
        public Cekler Cekler { get; set; }

        public Kasalar Kasalar { get; set; }
    }
}