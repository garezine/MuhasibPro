using Muhasebe.Domain.Common;
using Muhasebe.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemEntity
{
    [Table("DonemDBSec")]
    public class DonemDBSec : BaseEntity
    {
        [Required]
        public long FirmaId { get; set; }

        [Required]
        public long MaliDonemId { get; set; }

        [Required]
        public string DBName { get; set; }

        [Required]
        public string Directory { get; set; }
        [Required]
        [StringLength(500)]
        public string DBPath { get; set; }

        public DatabaseType DatabaseType { get; set; }

        public MaliDonem MaliDonem { get; set; }
    }
}
