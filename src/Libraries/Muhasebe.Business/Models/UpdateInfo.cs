namespace Muhasebe.Business.Models
{
    public class UpdateInfo
    {
        public bool HasUpdate { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string Description { get; set; }        
  
        public string LocalPath { get; set; }
        public bool IsDownloaded { get; set; }

        // Yeni alanlar
        public long FileSize { get; set; } // Dosya boyutu (bytes)
        public DateTime ReleaseDate { get; set; } // Release tarihi

        // Helper properties
        public string FormattedFileSize => FormatBytes(FileSize);
        public string FormattedReleaseDate => ReleaseDate.ToString("dd.MM.yyyy");

        private static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F1} {suffixes[suffixIndex]}";
        }
    }
}

