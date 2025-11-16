using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("PersonelTopluOdemeler")]
    public class PersonelTopluOdeme
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PersonelId { get; set; }

        [MaxLength(255)]
        public string Aciklama { get; set; }

        [MaxLength(100)]
        public string Adi { get; set; }

        [MaxLength(100)]
        public string Soyadi { get; set; }

        [MaxLength(50)]
        public string HesapDurumu { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal NetMaasi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal OdemeTutari { get; set; }

        public ICollection<Personeller> Personeller { get; set; }
    }
}