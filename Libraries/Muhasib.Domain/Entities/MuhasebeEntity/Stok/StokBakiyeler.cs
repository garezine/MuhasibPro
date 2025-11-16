using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Stok
{
    [Table("StokBakiyeler")]
    public class StokBakiyeler
    {
        [Key]
        [DatabaseGenerat‌​ed(DatabaseGeneratedOption.None)]
        public long StokId { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? ToplamStokGiris { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? ToplamStokCikis { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? MevcutStokBakiye { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? SonAlisFiyat { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal? SonSatisFiyat { get; set; }

        public Stoklar Stok { get; set; }
    }
}
