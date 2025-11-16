using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity;

[Table("Firmalar")]
public class Firma : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirmaKodu { get; set; }

    [Required]
    [MaxLength(100)]
    public string KisaUnvani { get; set; }

    [MaxLength(255)]
    public string? TamUnvani { get; set; }

    [Required]
    [MaxLength(100)]
    public string YetkiliKisi { get; set; }

    [MaxLength(50)]
    public string? Il { get; set; }

    [MaxLength(50)]
    public string? Ilce { get; set; }

    [MaxLength(255)]
    public string? Adres { get; set; }

    [MaxLength(25)]
    public string? PostaKodu { get; set; }

    [MaxLength(17)]
    public string? Telefon1 { get; set; }

    [MaxLength(17)]
    public string? Telefon2 { get; set; }

    [MaxLength(50)]
    public string? VergiDairesi { get; set; }

    [MaxLength(11)]
    public string? VergiNo { get; set; }

    [MaxLength(11)]
    public string? TCNo { get; set; }

    [MaxLength(75)]
    public string? Web { get; set; }

    [MaxLength(75)]
    public string? Eposta { get; set; }

    public byte[]? Logo { get; set; }
    public byte[]? LogoOnizleme { get; set; }

    [Required]
    [MaxLength(10)]
    public string PBu1 { get; set; } = "TL";

    [MaxLength(10)]
    public string? PBu2 { get; set; }
    public ICollection<MaliDonem> MaliDonemler { get; set; }

    public string BuildSearchTerms() => $"{Id} {KisaUnvani} {TamUnvani} {YetkiliKisi} {Telefon1}".ToLower();
}
