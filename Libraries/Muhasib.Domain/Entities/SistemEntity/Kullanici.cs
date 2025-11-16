using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity;
[Table("Kullanicilar")]
public class Kullanici : BaseEntity
{
    [MaxLength(50)]
    [Required]
    public string KullaniciAdi { get; set; }
    [MaxLength(400)]
    [Required]
    public string SifreHash { get; set; }
    [MaxLength(50)]
    [Required]
    public string Adi { get; set; }
    [Required]
    [MaxLength(50)]
    public string Soyadi { get; set; }
    [MaxLength(100)]
    [Required]
    public string Eposta { get; set; }
    [Required]
    public long RolId { get; set; }
    [MaxLength(17)]
    public string Telefon { get; set; }
    public byte[]? Resim { get; set; }
    public byte[]? ResimOnizleme { get; set; }
    public virtual KullaniciRol Rol { get; set; }
    public ICollection<Hesap> Hesaplar { get; set; }
}