using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("VarsayilanDegerler")]
    public class VarsayilanDegerler
    {
        [MaxLength(50)]
        public string CariHesapAranAlan { get; set; }

        [MaxLength(50)]
        public string CariHesapHareketSiralama { get; set; }

        [MaxLength(50)]
        public string CariHesapKartSiralama { get; set; }

        [MaxLength(50)]
        public string StokBulArananAlan { get; set; }

        [MaxLength(50)]
        public string StokBulKartSiralama { get; set; }
    }
}
