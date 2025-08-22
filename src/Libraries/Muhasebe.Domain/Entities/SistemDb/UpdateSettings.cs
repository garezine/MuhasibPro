using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemDb
{
    [Table("UpdateSettings")]
    public class UpdateSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public bool AutoCheckOnStartup { get; set; } = true;
        public bool AutoDownload { get; set; } = false;
     
        public bool AutoInstall { get; set; } = false;

        public int CheckIntervalHours { get; set; } = 24;

        public bool IncludeBetaVersions { get; set; } = false;

        public DateTime? LastCheckDate { get; set; }

        public bool ShowNotifications { get; set; } = true;

        public string UpdateChannel { get; set; } = "Stable";

        public string? PendingUpdateVersion { get; set; }
        public string? PendingUpdateLocalPath { get; set; }
        public string? PendingUpdateDownloadUrl { get; set; }
        public DateTime? PendingUpdateDownloadedAt { get; set; }
        public long PendingUpdateFileSize { get; set; }
        public string? PendingUpdateFileHash { get; set; }
    }
}
