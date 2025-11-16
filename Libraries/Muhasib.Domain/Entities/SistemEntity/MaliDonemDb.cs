using Muhasib.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("MaliDonemDbler")]
    public class MaliDonemDb : BaseEntity
    {

        [Required]
        [ForeignKey(nameof(MaliDonemId))]
        public long MaliDonemId { get; set; }

        [Required]
        public string DBName { get; set; }

        [Required]
        public string Directory { get; set; }
        [Required]
        [StringLength(1000)]
        public string DBPath { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public MaliDonem MaliDonem { get; set; }
        public string BuildSearchTerms() => $"{Id} {DBName} {DatabaseType.SqlServer}".ToLower();
    }
}
