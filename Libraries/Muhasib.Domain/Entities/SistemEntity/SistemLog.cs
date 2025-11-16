using Muhasib.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("SistemLogs")]
    public class SistemLog : BaseEntity
    {

        public bool IsRead { get; set; }
        [Required]
        [MaxLength(50)]
        public string User { get; set; }

        [Required]
        public LogType Type { get; set; }
        [Required]
        [MaxLength(50)]
        public string Source { get; set; }
        [Required]
        [MaxLength(50)]
        public string Action { get; set; }
        [Required]
        [MaxLength(400)]
        public string Message { get; set; }
        [MaxLength(4000)]
        public string? Description { get; set; }
    }
}
