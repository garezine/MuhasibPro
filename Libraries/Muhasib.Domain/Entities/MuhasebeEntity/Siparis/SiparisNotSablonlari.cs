using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Siparis
{
    [Table("SiparisNotSablonlar")]
    public class SiparisNotSablonlari : BaseEntity
    {
        public string Notlar { get; set; }

        [MaxLength(50)]
        public string SablonAdi { get; set; }

        [MaxLength(2)]
        public string Turu { get; set; }
    }
}