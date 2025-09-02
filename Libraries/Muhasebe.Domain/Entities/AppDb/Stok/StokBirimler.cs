using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.AppDb.Stok;

[Table("StokBirimler")]
public class StokBirimler : BaseEntity
{
    [MaxLength(25)] public string BirimAdi { get; set; }

    public ICollection<Stoklar> Stoklar { get; set; }
}
