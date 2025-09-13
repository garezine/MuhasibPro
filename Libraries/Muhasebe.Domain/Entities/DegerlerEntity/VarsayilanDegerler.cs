using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
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
