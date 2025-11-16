using System.ComponentModel.DataAnnotations;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    public class HatirlatmaTurler : BaseEntity
    {
        [MaxLength(50)] public string TurAdi { get; set; }
        public ICollection<Hatirlatmalar> Hatirlatmalar { get; set; }
    }
}