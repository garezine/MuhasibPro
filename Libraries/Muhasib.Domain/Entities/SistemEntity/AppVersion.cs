using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasib.Domain.Entities.SistemEntity
{
    [Table("AppVersiyonlar")]
    public class AppVersion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string CurrentAppVersion { get; set; }
        public DateTime CurrentAppVersionLastUpdate { get; set; }
        public string? PreviousAppVersiyon { get; set; }
    }

}

