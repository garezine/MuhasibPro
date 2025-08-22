namespace Muhasebe.Business.Models.UpdateModels
{
    public class DeltaUpdateInfo
    {
        public string[] ChangedFiles { get; set; } = Array.Empty<string>();
        public long DeltaSize { get; set; }
        public long FullSize { get; set; }
        public string DeltaDownloadUrl { get; set; } = string.Empty;
        public Dictionary<string, string> FileHashes { get; set; } = new();
        public bool IsDeltaAvailable { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string NewVersion { get; set; } = string.Empty;
        public int ChangedFilesCount { get; set; }
        public string ChangelogUrl { get; set; }
        public string ReleaseNotesUrl { get; set; }

        public int DeltaPercentage => FullSize > 0 ? (int)((double)DeltaSize / FullSize * 100) : 100;

        // Delta güncellemesi için gerekli ek özellikler
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Checksum { get; set; } = string.Empty;
        public bool IsRollbackSupported { get; set; } = true;
    }
}
