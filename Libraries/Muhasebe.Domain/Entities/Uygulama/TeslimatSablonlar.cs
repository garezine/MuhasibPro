using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama
{
    [Table("TeslimatSablonlar")]
    public class TeslimatSablonlar : BaseEntity
    {
        public string SablonAdi { get; set; }

    }
}
