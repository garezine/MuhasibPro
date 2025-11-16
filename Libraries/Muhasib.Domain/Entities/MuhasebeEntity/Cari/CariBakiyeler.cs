using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariBakiyeler")]
    public class CariBakiyeler
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CariId { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Borc { get; set; }//Gider

        [Column(TypeName = "decimal (18,2)")]
        public decimal Alacak { get; set; }//Gelir

        [Column(TypeName = "decimal (18,2)")]
        public decimal Bakiye { get; set; }

        [MaxLength(50)]
        public string BakiyeTipi { get; set; }
        public CariHesap Cari { get; set; }
    }
}