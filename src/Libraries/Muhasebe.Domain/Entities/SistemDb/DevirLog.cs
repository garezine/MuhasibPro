using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemDb
{
    [Table("DevirLog")]
    public class DevirLog : BaseEntity
    {
        public DateTime Tarih { get; set; }

        [MaxLength(50)]
        public string SirketKisaUnvan { get; set; }

        [MaxLength(255)]
        public string SirketTamUnvan { get; set; }

        [MaxLength(10)]
        public string KaynakDonem { get; set; }

        [MaxLength(10)]
        public string HedefDonem { get; set; }

        [MaxLength(255)]
        public string DevirNotu { get; set; }

        public long SirketId { get; set; }

        public string Moduller { get; set; }
    }
}
