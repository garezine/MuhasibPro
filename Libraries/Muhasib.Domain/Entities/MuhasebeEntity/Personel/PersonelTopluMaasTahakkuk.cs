using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("PersonelTopluMaasTahakkuk")]
    public class PersonelTopluMaasTahakkuk
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PersonelId { get; set; }

        [MaxLength(100)]
        public string Adi { get; set; }

        [MaxLength(100)]
        public string Soyadi { get; set; }

        public short CalismadigiGunSayisi { get; set; }

        public float CalsmadigiGunKesintisi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal GunlukUcret { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Maasi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal SaatlikUcret { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal TahakkukEdenMaas { get; set; }

        public ICollection<Personeller> Personeller { get; set; }
    }
}