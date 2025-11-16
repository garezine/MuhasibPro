using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("AppVersiyonlar")]
    public class AppVersiyon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MevcutVersiyon { get; set; }
        public DateTime UygulamaSonGuncellemeTarihi { get; set; }
        public string? OncekiUygulamaVersiyon { get; set; }
    }

}

