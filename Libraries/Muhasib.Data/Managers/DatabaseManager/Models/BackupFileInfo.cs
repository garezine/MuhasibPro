namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public class BackupFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public long SizeBytes { get; set; }
        public string SizeFormatted { get; set; }
    }
}
