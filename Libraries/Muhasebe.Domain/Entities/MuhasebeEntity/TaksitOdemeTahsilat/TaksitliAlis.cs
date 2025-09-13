using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.MuhasebeEntity.Cari;
using System.ComponentModel.DataAnnotations.Schema;


namespace Muhasebe.Domain.Entities.MuhasebeEntity.TaksitOdemeTahsilat
{
    [Table("TaksitliAlis")]
    public class TaksitliAlis : BaseEntity
    {
        public long CariId { get; set; }

        public long HareketId { get; set; }

        public short FaizOrani { get; set; }

        public short GunSayisi { get; set; }

        public DateTime IlkTaksitTarihi { get; set; }

        public short KalanTaksitSay { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal Pesinat { get; set; }

        [Column(TypeName = "decimal (18,2)")]
        public decimal SatisTutari { get; set; }

        public short TaksitSayisi { get; set; }
        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}
