using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Sistem;

[Table("DevirLogII")]
public class DevirLogII : BaseEntity
{
    [MaxLength(255)]
    public string DevirAciklama { get; set; }

    [MaxLength(255)]
    public string A1 { get; set; }

    [MaxLength(255)]
    public string A2 { get; set; }

    [MaxLength(255)]
    public string A3 { get; set; }

    [MaxLength(255)]
    public string A4 { get; set; }

    public int DevirYili { get; set; }

}
