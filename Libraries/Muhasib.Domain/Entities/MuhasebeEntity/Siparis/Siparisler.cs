using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Siparis
{
    [Table("Siparisler")]
    public class Siparisler : BaseEntity
    {
        [MaxLength(255)]
        public string Aciklama { get; set; }

        public long CariId { get; set; }

        public string Notlar { get; set; }

        [MaxLength(50)]
        public string SiparisDurumu { get; set; }

        [MaxLength(50)]
        public string SiparisNo { get; set; }

        public DateTime SiparisSaati { get; set; }

        public DateTime SiparisTarihi { get; set; }

        [MaxLength(20)]
        public string SiparisTuru { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal SiparisTutari { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }

        public ICollection<SiparisDetay> SiparisDetaylar { get; set; }

        public ICollection<SiparisNotSablonlari> SiparisNotSablonlari { get; set; }
    }
}
