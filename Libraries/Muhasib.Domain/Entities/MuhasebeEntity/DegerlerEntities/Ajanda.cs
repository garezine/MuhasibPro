using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;

[Table("Ajandalar")]
public class Ajanda : BaseEntity
{
    [Required]
    public string Metin { get; set; }
    [Required]
    public DateTime Tarih { get; set; }
}