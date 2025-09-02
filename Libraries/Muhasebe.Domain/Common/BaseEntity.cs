using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    public long KaydedenId { get; set; }

    [Required]
    public DateTimeOffset KayitTarihi { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? GuncellemeTarihi { get; set; }

    public long? GuncelleyenId { get; set; }

    public bool AktifMi { get; set; } = true;

    public string? ArananTerim { get; set; }
}
