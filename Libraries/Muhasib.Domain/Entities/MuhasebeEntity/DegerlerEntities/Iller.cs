using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("Iller")]
    public class Iller : BaseEntity
    {
        [MaxLength(50)]
        public string IlAdi { get; set; }
    }
}