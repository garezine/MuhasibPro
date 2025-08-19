using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemDb;

[Table("Hesaplar")]
public class Hesap : BaseEntity
{
    [Required]
    public long KullaniciId { get; set; }

    [Required]
    public long FirmaId { get; set; }

    [Required]
    public long DonemId { get; set; }

    public DateTime SonGirisTarihi { get; set; }

    public Kullanici Kullanici { get; set; }
}


