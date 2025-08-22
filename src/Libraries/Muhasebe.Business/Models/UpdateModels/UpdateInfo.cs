namespace Muhasebe.Business.Models.UpdateModels
{
    public class UpdateInfo
    {
        // Mevcut property'ler...
        public bool HasUpdate { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public string LatestVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string ReleaseNotes { get; set; }
        public string DownloadUrl { get; set; }
        public long FileSize { get; set; }
        public DateTime? ReleaseDate { get; set; }

        // Yeni property'ler
        public string ChangelogUrl { get; set; }
        public string ReleaseNotesUrl { get; set; }
        public string FileHash { get; set; }
        public bool IsCriticalUpdate { get; set; }
        public bool RequiresRestart { get; set; } = true;

        // Helper properties
        public string FormattedFileSize => FormatFileSize(FileSize);
        public string FormattedReleaseDate => ReleaseDate?.ToString("dd.MM.yyyy") ?? "";

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB" };
            int i = 0;
            double size = bytes;

            while (size >= 1024 && i < sizes.Length - 1)
            {
                size /= 1024;
                i++;
            }

            return $"{size:F1} {sizes[i]}";
        }
    }
}

