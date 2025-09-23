using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity
{
    [Table("AppVersiyon")]
    public class AppVersiyon
    {
        [Key]
        public string UygulamaVersiyon { get; set; }
        public DateTime UygulamaSonGuncellemeTarihi { get; set; }
        public string? OncekiUygulamaVersiyon { get; set; }
    }

}

