using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity;

[Table("Degerler")]
public class Degerler : BaseEntity
{
    public long SonSecilenSirket { get; set; }

    public long SonSecilenDonem { get; set; }

    public long SonSecilenKullanici { get; set; }
}
