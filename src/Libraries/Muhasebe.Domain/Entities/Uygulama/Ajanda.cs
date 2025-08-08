using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama;

[Table("Ajandalar")]
public class Ajanda : BaseEntity
{
    [Required]
    public string Metin { get; set; }
    [Required]
    public DateTime Tarih { get; set; }
}