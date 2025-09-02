using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.Uygulama
{
    [Table("ParaBirimler")]
    public class ParaBirimler : BaseEntity
    {
        public string Kisaltmasi { get; set; }
        public string PB { get; set; }
    }
}