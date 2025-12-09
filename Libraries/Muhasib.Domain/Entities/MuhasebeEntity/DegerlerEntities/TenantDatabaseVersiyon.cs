using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.MuhasebeEntity.DegerlerEntities
{
    [Table("TenantDatabaseVersions")]
    public class TenantDatabaseVersiyon
    {
        [Key]
        public string DatabaseName { get; set; }       
        public string TenantDbVersion { get; set; } = "1.0.0";
        public DateTime TenantDbLastUpdate { get; set; } = DateTime.Now;
        public string? PreviousTenantDbVersiyon { get; set; }

    }
}

