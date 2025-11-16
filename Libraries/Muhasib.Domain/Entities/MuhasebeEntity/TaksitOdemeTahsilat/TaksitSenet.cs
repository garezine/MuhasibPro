using Muhasib.Domain.Entities.MuhasebeEntity.Cari;
using Muhasib.Domain.Entities.MuhasebeEntity.Kasa;
using Muhasib.Domain.Entities.MuhasebeEntity.Senet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.TaksitOdemeTahsilat
{
    [Table("TaksitSenet")]
    public class TaksitSenet : BaseEntity
    {
        public long HareketId { get; set; }

        [MaxLength(150)]
        public string AlacakliUnvani { get; set; }

        [MaxLength(50)]
        public string IhtilafMahkemesi { get; set; }

        [MaxLength(255)]
        public string KefilAdres { get; set; }

        [MaxLength(150)]
        public string KefilIsim { get; set; }

        [MaxLength(11)]
        public short KefilTCKimlikNo { get; set; }

        [MaxLength(11)]
        public short KefilVDN { get; set; }

        [MaxLength(255)]
        public string OdeyecekAdres1 { get; set; }

        [MaxLength(255)]
        public string OdeyecekAdres2 { get; set; }

        [MaxLength(150)]
        public string OdeyecekIsim { get; set; }

        [MaxLength(11)]
        public short OdeyecekTCKimlikno { get; set; }

        [MaxLength(11)]
        public short OdeyecekVDN { get; set; }

        public ICollection<Senetler> Senetler { get; set; }

        public ICollection<KasaHareket> kasaHareketler { get; set; }

        public ICollection<CariHesap> CariKartlar { get; set; }
    }
}