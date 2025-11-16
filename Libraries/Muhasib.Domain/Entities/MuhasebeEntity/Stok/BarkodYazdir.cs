using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Stok
{
    [Table("BarkodYazdir")]
    public class BarkodYazdir
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BarkodId { get; set; }

        public string BarkodTipi { get; set; }

        [MaxLength(50)]
        public string CiktiKonumu { get; set; }

        [MaxLength(50)]
        public short SatirSayisi { get; set; }

        public virtual Barkod Barkod { get; set; }
    }
}
