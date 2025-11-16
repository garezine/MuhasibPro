using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using System.ComponentModel.DataAnnotations;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Fatura_Irsaliye
{
    public class Irsaliyeler : BaseEntity
    {
        public long CariHesapId { get; set; }
        public long? FaturaId { get; set; }
        [MaxLength(50)] public string IrsaliyeTipi { get; set; }
        [MaxLength(50)] public string IrsaliyeNo { get; set; }

        public DateTime IslemTarihi { get; set; }
        public DateTime IrsaliyeTarihi { get; set; }
        public DateTime SevkTarihi { get; set; }

        public string TeslimAlanKisi { get; set; }
        public string TeslimEdenKisi { get; set; }
        public string Aciklama { get; set; }

        public CariHesap CariHesap { get; set; }
        public Faturalar Fatura { get; set; }
        public ICollection<IrsaliyeKalemler> IrsaliyeKalemler { get; set; }

    }
}
