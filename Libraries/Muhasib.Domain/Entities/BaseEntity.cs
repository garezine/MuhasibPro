using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Required]
    public long KaydedenId { get; set; }

    [Required]
    public DateTime KayitTarihi { get; set; }

    public DateTime? GuncellemeTarihi { get; set; }

    public long? GuncelleyenId { get; set; }

    public bool AktifMi { get; set; } = true;

    public string? ArananTerim { get; set; }
}
