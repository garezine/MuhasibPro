using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Personel
{
    [Table("PersonelGorevler")]
    public class PersonelGorev : BaseEntity
    {
        [MaxLength(50)]
        public string GorevAdi { get; set; }
        public ICollection<Personeller> Personeller { get; set; }
    }
}