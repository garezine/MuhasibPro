using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Personel
{
    [Table("PersonelBolum")]
    public class PersonelBolum : BaseEntity
    {
        [MaxLength(50)]
        public string BolumAdi { get; set; }
        public ICollection<Personeller> Personeller { get; set; }
    }
}
