namespace Muhasebe.Business.Models.UpdateModels
{
    public class PendingUpdateInfo
    {
        public UpdateInfo UpdateInfo { get; set; }
        public string LocalPath { get; set; }
        public DateTime DownloadedAt { get; set; }
        public long FileSize { get; set; }
        public string FileHash { get; set; } // MD5 veya SHA256
        public bool IsVerified { get; set; }
    }
}
