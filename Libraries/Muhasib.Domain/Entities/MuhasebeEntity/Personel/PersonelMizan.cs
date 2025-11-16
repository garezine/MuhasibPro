using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("PersonelMizan")]
    public class PersonelMizan : BaseEntity
    {
        public long PersonelId { get; set; }

        [MaxLength(100)]
        public string Adi { get; set; }

        [MaxLength(100)]
        public string Soyadi { get; set; }

        [MaxLength(10)]
        public string BakiyeTipi { get; set; }

        [MaxLength(50)]
        public string Bolumu { get; set; }

        [MaxLength(17)]
        public string CepTelefonu { get; set; }

        [MaxLength(10)]
        public string Cinsiyeti { get; set; }

        [MaxLength(50)]
        public string Gorevi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal HesapBakiye { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal NetMaasi { get; set; }

        public ICollection<Personeller> Personeller { get; set; }
    }
}
