using Muhasebe.Business.Models.UpdateModels;

namespace MuhasibPro.Core.Models.Update
{
    public enum UpdateState
    {
        Idle,
        Checking,
        UpdateAvailable,
        Downloading,
        Downloaded,
        Installing,
        Installed,
        Error
    }

    public class UpdateEventArgs
    {
        public UpdateState State { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public UpdateInfo UpdateInfo { get; set; }
        public Exception Error { get; set; }

        public UpdateEventArgs(UpdateState state, string message = null)
        {
            State = state;
            Message = message;
        }

        public UpdateEventArgs(UpdateState state, UpdateInfo updateInfo, string message = null)
        {
            State = state;
            UpdateInfo = updateInfo;
            Message = message;
        }

        public UpdateEventArgs(Exception error, string message = null)
        {
            State = UpdateState.Error;
            Error = error;
            Message = message ?? error.Message;
        }
    }

    public class UpdateProgressEventArgs
    {
        public int Percentage { get; set; }
        public long Downloaded { get; set; }
        public long Total { get; set; }
        public double Speed { get; set; }
        public string Status { get; set; }
        public UpdateState CurrentState { get; set; }

        // State + percentage constructor (kurulum için)
        public UpdateProgressEventArgs(UpdateState state, int percentage = 0, string status = null)
        {
            CurrentState = state;
            Percentage = percentage;
            Status = status;
        }

        // State olmadan - sadece progress bilgileri için (DÜZELTME: Percentage hesabı eklendi)
        public UpdateProgressEventArgs(long downloaded, long total, double speed)
        {
            Downloaded = downloaded;
            Total = total;
            Speed = speed;
            // Percentage hesabı eklendi
            Percentage = total > 0 ? (int)((double)downloaded / total * 100) : 0;
            CurrentState = UpdateState.Downloading; // Default state
        }

        // Full constructor (state + progress bilgileri)
        public UpdateProgressEventArgs(UpdateState state, long downloaded, long total, double speed, string status = null)
        {
            CurrentState = state;
            Downloaded = downloaded;
            Total = total;
            Speed = speed;
            Percentage = total > 0 ? (int)((double)downloaded / total * 100) : 0;
            Status = status;
        }

        // Formatting properties
        public string FormattedDownloaded => FormatBytes(Downloaded);
        public string FormattedTotal => FormatBytes(Total);
        public string FormattedSpeed => $"{FormatBytes((long)Speed)}/s";

        private static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
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

    // MessageService Events İçin Sabitler
    public static class UpdateEvents
    {
        public const string StateChanged = "UpdateStateChanged";
        public const string Progress = "UpdateProgress";
        public const string Error = "UpdateError";
        public const string PendingUpdateChanged = "PendingUpdateChanged";
    }
}