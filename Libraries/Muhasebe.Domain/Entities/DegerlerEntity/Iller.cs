using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
{
    [Table("Iller")]
    public class Iller : BaseEntity
    {
        [MaxLength(50)]
        public string IlAdi { get; set; }
    }
}