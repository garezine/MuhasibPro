using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Stok
{
    [Table("StokGruplar")]
    public class StokGruplar : BaseEntity
    {
        [MaxLength(50)] public string GrupAdi { get; set; }
        public ICollection<Stoklar> Stoklar { get; set; }
    }
}
