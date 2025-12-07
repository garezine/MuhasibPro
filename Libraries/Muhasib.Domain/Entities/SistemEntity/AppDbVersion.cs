using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("AppDbVersiyonlar")]
    public class AppDbVersion : AppVersion
    {       
        public string CurrentDatabaseVersion { get; set; }
        public DateTime CurrentDatabaseLastUpdate { get; set; }
        public string? PreviousDatabaseVersion { get; set; }
    }
}
