using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Sistem;

[Table("CalismaDonemler")]
public class CalismaDonem : BaseEntity
{
    [Required]
    public long FirmaId { get; set; }

    [Required]
    public int CalismaYilDonem { get; set; }

    public CalismaDonemSec CalismaDonemDb { get; set; }
    public Firma Firma { get; set; }
}
