using Muhasebe.Domain.Common;
using Muhasebe.Domain.Entities.Cari;
using Muhasebe.Domain.Entities.Kasa;
using Muhasebe.Domain.Entities.Senet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.TaksitOdemeTahsilat
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