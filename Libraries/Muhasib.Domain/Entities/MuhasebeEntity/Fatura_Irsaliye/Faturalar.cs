using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye
{
    public class Faturalar : BaseEntity
    {
        public long CariHesapId { get; set; }

        [MaxLength(50)] public string FaturaTipi { get; set; }

        [MaxLength(50)] public string FaturaNo { get; set; }

        public DateTime IslemTarihi { get; set; }

        public DateTime FaturaTarihi { get; set; }

        public bool IrsaliyeOlustur { get; set; }

        [MaxLength(50)] public string OdemeDurum { get; set; }

        [MaxLength(50)] public string OdemeTuru { get; set; }

        [MaxLength(50)] public string BelgeNo { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal ToplamTutar { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal ToplamIskontoTutar { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal AraToplam { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal KDVTutari { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal GenelToplamTutar { get; set; }

        [MaxLength(400)] public string Aciklama { get; set; }

        public CariHesap CariHesap { get; set; }
        public ICollection<CariHareketler> CariHareketler { get; set; }
        public ICollection<FaturaKalemler> FaturaKalemler { get; set; }
        public ICollection<Irsaliyeler> Irsaliyeler { get; set; }
    }
}
