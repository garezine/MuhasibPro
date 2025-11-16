using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("OdemeTurler")]
    public class OdemeTurler : BaseEntity
    {
        [MaxLength(30)] public string OdemeTuru { get; set; }
    }
}
