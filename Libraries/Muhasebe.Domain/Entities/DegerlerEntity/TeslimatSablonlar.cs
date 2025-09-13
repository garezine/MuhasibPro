using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
{
    [Table("TeslimatSablonlar")]
    public class TeslimatSablonlar : BaseEntity
    {
        public string SablonAdi { get; set; }

    }
}
