using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("PersonelHareketler")]
    public class PersonelHareket : BaseEntity
    {
        public long PersonelId { get; set; }

        public DateTime IslemTarihi { get; set; }

        [MaxLength(50)]
        public string IslemTipi { get; set; }

        [MaxLength(50)]
        public string HesapSekli { get; set; }

        [MaxLength(50)]
        public string BelgeNo { get; set; }

        [MaxLength(150)]
        public string Aciklama { get; set; }

        public bool GC { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal IslemTutari { get; set; }
        public Personeller Personel { get; set; }

    }
}
