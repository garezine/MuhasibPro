using Muhasib.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity;

[Table("MaliDonemler")]
public class MaliDonem : BaseEntity
{
    [Required]    
    public long FirmaId { get; set; }

    [Required]
    public int MaliYil { get; set; }

   
    [Required]
    public string DBName { get; set; }

    [Required]
    public string Directory { get; set; }
    [Required]
    [StringLength(1000)]
    public string DBPath { get; set; }
    public DatabaseType DatabaseType { get; set; }
   
    public Firma Firma { get; set; }
    public string BuildSearchTerms() => $"{Id} {FirmaId} {MaliYil}".ToLower();
}
