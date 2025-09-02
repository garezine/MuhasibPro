using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama
{
    [Table("APP_FormPozisyon")]
    public class APP_FormPozisyon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [MaxLength(50)] public string FormAdi { get; set; }

        public short Left { get; set; }

        public short Top { get; set; }
    }
}
