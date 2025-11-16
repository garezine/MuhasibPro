using Muhasib.Domain.Entities.MuhasebeEntity.Banka;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cek
{
    [Table("CekHesaplari")]
    public class CekHesaplari : BaseEntity
    {
        public long BankaId { get; set; }
        [MaxLength(50)]
        public string BankaAdi { get; set; }

        [MaxLength(25)]
        public string HesapNo { get; set; }

        [MaxLength(50)]
        public string Ili { get; set; }

        [MaxLength(5)]
        public string PB { get; set; }

        [MaxLength(15)]
        public string SubeKodu { get; set; }

        [MaxLength(50)]
        public string Subesi { get; set; }

        public ICollection<BankaHesaplar> Bankalar { get; set; }
    }
}