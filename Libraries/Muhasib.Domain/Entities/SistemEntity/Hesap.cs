using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity;

[Table("Hesaplar")]
public class Hesap : BaseEntity
{
    [ForeignKey("Kullanici")]
    public long KullaniciId { get; set; }
    public DateTime SonGirisTarihi { get; set; }
    public Kullanici Kullanici { get; set; }
}


