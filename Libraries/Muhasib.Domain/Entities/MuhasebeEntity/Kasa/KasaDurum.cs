using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Kasa
{
    [Table("KasaDurumlar")]
    public class KasaDurum
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long KasaId { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Gider { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Gelir { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Bakiye { get; set; }

        [MaxLength(50)]
        public string BakiyeTipi { get; set; }
        public Kasalar Kasa { get; set; }
    }
}
