using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariHesapDetaylar")]
    public class CariHesapDetay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CariId { get; set; }

        public float? AlisIskontosu { get; set; }

        public float? SatisIskontosu { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BorcLimiti { get; set; }


        [MaxLength(50)]
        public string OzelAlan_1 { get; set; }

        [MaxLength(50)]
        public string OzelAlan_2 { get; set; }

        public CariHesap Cari { get; set; }
    }
}