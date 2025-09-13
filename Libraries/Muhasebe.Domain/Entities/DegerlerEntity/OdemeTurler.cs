using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
{
    [Table("OdemeTurler")]
    public class OdemeTurler : BaseEntity
    {
        [MaxLength(30)] public string OdemeTuru { get; set; }
    }
}
