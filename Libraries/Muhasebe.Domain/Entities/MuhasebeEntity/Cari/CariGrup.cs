using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.MuhasebeEntity.Cari
{
    [Table("CariGruplar")]
    public class CariGrup : BaseEntity
    {
        [MaxLength(50)] public string GrupAdi { get; set; }
        public ICollection<CariHesap> Cariler { get; set; }
    }
}