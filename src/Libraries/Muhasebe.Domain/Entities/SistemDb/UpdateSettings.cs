using Muhasebe.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muhasebe.Domain.Entities.SistemDb
{
    [Table("UpdateSettings")]
    public class UpdateSettings : BaseEntity
    {
        /// <summary>
        /// Uygulama başlangıcında otomatik güncelleme kontrolü
        /// </summary>
        public bool AutoCheckOnStartup { get; set; } = true;

        /// <summary>
        /// Güncellemeyi otomatik indir (sadece kontrol etme)
        /// </summary>
        public bool AutoDownload { get; set; } = false;

        /// <summary>
        /// Güncellemeyi otomatik kur (kullanıcıya sormadan)
        /// </summary>
        public bool AutoInstall { get; set; } = false;

        /// <summary>
        /// Güncelleme kontrolü aralığı (saat cinsinden)
        /// </summary>
        public int CheckIntervalHours { get; set; } = 24;

        /// <summary>
        /// Beta sürümleri kontrol et
        /// </summary>
        public bool IncludeBetaVersions { get; set; } = false;

        /// <summary>
        /// Son güncelleme kontrolü tarihi
        /// </summary>
        public DateTime? LastCheckDate { get; set; }

        /// <summary>
        /// Kullanıcı ID (eğer multi-user sistemse)
        /// </summary>

        /// <summary>
        /// Bildirim göster
        /// </summary>
        public bool ShowNotifications { get; set; } = true;

        /// <summary>
        /// Hangi güncelleme kanalı (Stable, Beta, Alpha)
        /// </summary>
        public string UpdateChannel { get; set; } = "Stable";
    }
}
