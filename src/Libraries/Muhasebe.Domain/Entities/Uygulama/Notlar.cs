using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama
{
    [Table("Notlar")]
    public class Notlar : BaseEntity
    {
        [MaxLength(20)]
        public string FormPozisyon { get; set; }

        public string Not { get; set; }

        [MaxLength(50)]
        public DateTime Tarih { get; set; }
    }
}
