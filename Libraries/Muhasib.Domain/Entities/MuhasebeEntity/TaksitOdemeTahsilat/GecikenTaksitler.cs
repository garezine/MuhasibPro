using Muhasib.Domain.Entities.MuhasebeEntity.Stok;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.TaksitOdemeTahsilat
{
    [Table("GecikenTaksitler")]
    public class GecikenTaksitler
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TaksitId { get; set; }

        [MaxLength(20)]
        public string Durumu { get; set; }

        public DateTime OdemeTarihi { get; set; }

        public long StokId { get; set; }

        [MaxLength(10)]
        public string TaksitNo { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal TaksitTutari { get; set; }

        public ICollection<Stoklar> StokKartlar { get; set; }

        public ICollection<Taksitler> Taksitler { get; set; }
    }
}