using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama
{
    [Table("DBVersiyonlar")]
    public class DBVersiyon
    {
        [Key]
        public string Versiyon { get; set; }
    }
}
