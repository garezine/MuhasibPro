using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Stok
{
    [Table("Barkodlar")]
    public class Barkod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [MaxLength(100)]
        public string StokKodu { get; set; }

        [MaxLength(255)]
        public string StokAdi { get; set; }

        public long StokId { get; set; }

        public ICollection<Stoklar> Stoklar { get; set; }
    }
}