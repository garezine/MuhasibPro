using System.ComponentModel.DataAnnotations;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    public class MuhasebeVersiyon
    {
        [Key]
        public string DatabaseName { get; set; }


        public string MuhasebeDBVersiyon { get; set; }
        public DateTime MuhasebeDBSonGuncellemeTarihi { get; set; }
        public string OncekiMuhasebeDbVersiyon { get; set; }

        // Navigation properties (ileride eklenebilir)
        // public string UygulamaVersiyon { get; set; } // SistemDB'den alınacak
    }
}
