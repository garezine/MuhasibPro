using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.AppDb.Cari;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Teklif
{
    [Table("Teklifler")]
    public class Teklifler : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        public long CariId { get; set; }

        [MaxLength(50)]
        public string Durumu { get; set; }

        public DateTime GecerlilikTarihi { get; set; }

        public string Notlar { get; set; }

        public DateTime OnaylanmaTarihi { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal TeklifFiyati { get; set; }

        [MaxLength(50)]
        public string TeklifTuru { get; set; }

        [MaxLength(25)]
        public string TeklifNo { get; set; }

        public DateTime TeklifTarihi { get; set; }

        [MaxLength(50)]
        public string TeslimatSuresi { get; set; }


        public ICollection<TeklifDetay> TeklifDetaylar { get; set; }

        public ICollection<TeklifNotSablonlari> TeklifNotSablonlari { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}