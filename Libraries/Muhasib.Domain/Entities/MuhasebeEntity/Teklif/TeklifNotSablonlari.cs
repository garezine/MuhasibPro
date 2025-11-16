using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Teklif
{
    [Table("TeklifNotSablonlari")]
    public class TeklifNotSablonlari : BaseEntity
    {
        public string Notlar { get; set; }

        [MaxLength(50)]
        public string SablonAdi { get; set; }

        [MaxLength(2)]
        public string Turu { get; set; }
    }
}
