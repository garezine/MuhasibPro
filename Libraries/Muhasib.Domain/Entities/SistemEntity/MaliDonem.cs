using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity;

[Table("MaliDonemler")]
public class MaliDonem : BaseEntity
{
    [Required]
    [ForeignKey("Firma")]
    public long FirmaId { get; set; }

    [Required]
    public int MaliYil { get; set; }

    [Required]
    public bool DbOlusturulduMu { get; set; } = false;

    public MaliDonemDb MaliDonemDb { get; set; }
    public Firma Firma { get; set; }
    public string BuildSearchTerms() => $"{Id} {FirmaId} {MaliYil}".ToLower();
}
