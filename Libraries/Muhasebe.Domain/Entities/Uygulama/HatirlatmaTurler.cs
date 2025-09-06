using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Muhasebe.Domain.Entities.Uygulama
{
    public class HatirlatmaTurler : BaseEntity
    {
        [MaxLength(50)] public string TurAdi { get; set; }
        public ICollection<Hatirlatmalar> Hatirlatmalar { get; set; }
    }
}