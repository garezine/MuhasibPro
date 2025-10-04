using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity
{
    [Table("AppVersiyonlar")]
    public class AppVersiyon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UygulamaVersiyon { get; set; }
        public DateTime UygulamaSonGuncellemeTarihi { get; set; }
        public string? OncekiUygulamaVersiyon { get; set; }
    }

}

