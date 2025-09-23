using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity;

[Table("CalismaDonemler")]
public class MaliDonem : BaseEntity
{
    [Required]
    public long FirmaId { get; set; }

    [Required]
    public int MaliDonemYil { get; set; }

    public DonemDBSec DonemDBSec { get; set; }
    public Firma Firma { get; set; }
}
