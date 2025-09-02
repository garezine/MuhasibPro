using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Siparis
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