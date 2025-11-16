using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Stok
{
    [Table("IstatistikKarZarar")]
    public class IstatistikKarZarar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public float AlisMiktari { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal AlisTutari { get; set; }

        [MaxLength(50)]
        public string birimi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal BrutKar { get; set; }

        public float SatisMiktari { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal SatisTutari { get; set; }

        public float StokBakiyesi { get; set; }

        [MaxLength(200)]
        public string StokAdi { get; set; }

        [MaxLength(50)]
        public string StokGrubu { get; set; }

        [MaxLength(50)]
        public string StokKodu { get; set; }

        public long StokId { get; set; }

        public long StokGrupId { get; set; }

        public ICollection<Stoklar> StokKartlar { get; set; }
    }
}