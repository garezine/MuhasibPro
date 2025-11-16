namespace Muhasib.Data.Managers.UpdataManager
{
    public class UpdateSettingsModel
    {
        public bool AutoCheckOnStartup { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public bool IncludeBetaVersions { get; set; } = false;
        public DateTime? LastCheckTime { get; set; }
    }
}
