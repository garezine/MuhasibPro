using Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariBankaHesaplar")]
    public class CariBankaHesap : BaseEntity
    {
        public long CariId { get; set; }

        [MaxLength(75)]
        public string BankaAdi { get; set; }

        [MaxLength(75)]
        public string Sube { get; set; }

        [MaxLength(50)]
        public string HesapNo { get; set; }

        [MaxLength(150)]
        public string HesapSahibi { get; set; }

        public long ParaBirimId { get; set; }
        public CariHesap Cari { get; set; }
        public ParaBirimler ParaBirim { get; set; }

    }
}