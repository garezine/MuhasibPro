using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
{
    public class MuhasebeVersiyon
    {
        [Key]
        [Column(Order = 0)]
        public string FirmaKodu { get; set; }

        [Key]
        [Column(Order = 1)]
        public int MaliDonemYil { get; set; }

        public string MuhasebeDBVersiyon { get; set; }
        public DateTime MuhasebeDBSonGuncellemeTarihi { get; set; }
        public string OncekiMuhasebeDbVersiyon { get; set; }

        // Navigation properties (ileride eklenebilir)
        // public string UygulamaVersiyon { get; set; } // SistemDB'den alınacak
    }
}
