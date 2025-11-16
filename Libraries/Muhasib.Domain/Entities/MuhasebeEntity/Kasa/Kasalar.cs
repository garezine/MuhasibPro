using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Kasa
{
    public class Kasalar : BaseEntity
    {
        [MaxLength(255)]
        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Bakiye { get; set; } = 0;

        [MaxLength(50)]
        public string KasaAdi { get; set; }
        public KasaDurum KasaDurum { get; set; }
        public ICollection<KasaHareket> KasaHareketler { get; set; }

    }
}