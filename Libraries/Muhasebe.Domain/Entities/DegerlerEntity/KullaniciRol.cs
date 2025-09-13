using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.DegerlerEntity
{
    [Table("KullaniciRol")]
    public class KullaniciRol : BaseEntity
    {
        public string RolAdi { get; set; }
        public string Aciklama { get; set; }
    }
}
