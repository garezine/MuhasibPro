using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.Personel
{
    [Table("PersonelBolum")]
    public class PersonelBolum : BaseEntity
    {
        [MaxLength(50)]
        public string BolumAdi { get; set; }
        public ICollection<Personeller> Personeller { get; set; }
    }
}
