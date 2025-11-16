using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("Hatirlatmalar")]
    public class Hatirlatmalar : BaseEntity
    {
        public string Aciklama { get; set; }

        public long? CariGrupId { get; set; }

        public long? CariId { get; set; }

        public long? CariHareketId { get; set; }

        public bool? Durum { get; set; }

        public int HatirlatmaTurleriId { get; set; }

        [MaxLength(10)] public string IdSTR { get; set; }

        [MaxLength(50)] public string Konu { get; set; }

        public DateTime? Tarih { get; set; }

        public ICollection<HatirlatmaTurler> HatirlatmaTurleri { get; set; }

    }
}
