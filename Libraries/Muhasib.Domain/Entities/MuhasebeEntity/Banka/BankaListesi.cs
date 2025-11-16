using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Banka
{
    [Table("BankaListesi")]
    public class BankaListesi : BaseEntity
    {
        [MaxLength(100)]
        public string BankaAdi { get; set; }
        public ICollection<BankaHesaplar> BankaHesaplar { get; set; }

    }
}