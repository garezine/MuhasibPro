using System.Text.Json;

namespace MuhasibPro.Helpers
{
    public class UpdateSettingsModel
    {
        public bool AutoCheckOnStartup { get; set; } = true;

        public int CheckIntervalHours { get; set; } = 24;

        public bool ShowNotifications { get; set; } = false;

        public bool AutoDownload { get; set; } = false;

        public bool IncludeBetaVersions { get; set; } = false;

        public bool AllowPrereleaseVersions { get; set; } = false;

        public DateTime? LastCheckDate { get; set; }
    }

    public static class UpdateHelper
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuhasibPro", "update-settings.json");

        public static UpdateSettingsModel Settings { get; private set; } = new UpdateSettingsModel();
        public static async Task<UpdateSettingsModel> LoadAsync()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    await SaveAsync(Settings);
                    return Settings;
                }

                var json = await File.ReadAllTextAsync(SettingsPath);
                return JsonSerializer.Deserialize<UpdateSettingsModel>(json) ?? new UpdateSettingsModel();
            }
            catch
            {
                return new UpdateSettingsModel();
            }
        }

        public static async Task SaveAsync(UpdateSettingsModel model)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SettingsPath, json);
            }
            catch
            {
                // Silent fail - settings not critical
            }
        }
    }
}
