using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemDb;

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

    // Kullanıcının rolü (örneğin: Admin, Kullanıcı, Editör vb.)
    [MaxLength(50)]
    [Required]
    public string Rol { get; set; }

    // Kullanıcının telefon numarası
    [MaxLength(17)]
    public string Telefon { get; set; }
    public ICollection<Hesap> Hesaplar { get; set; }
}