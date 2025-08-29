using System.Text.Json;

namespace MuhasibPro.Core.Models.Update
{
    public class UpdateSettings
    {
        public bool AutoCheckOnStartup { get; set; } = true;
        public int CheckIntervalHours { get; set; } = 24;
        public bool ShowNotifications { get; set; } = false;
        public bool AutoDownload { get; set; } = false;
        public bool IncludeBetaVersions { get; set; } = false;
        public bool AllowPrereleaseVersions { get; set; } = false;
        public DateTime? LastCheckDate { get; set; }

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuhasibPro", "update-settings.json");

        public static async Task<UpdateSettings> LoadAsync()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    var defaultSettings = new UpdateSettings();
                    await defaultSettings.SaveAsync();
                    return defaultSettings;
                }

                var json = await File.ReadAllTextAsync(SettingsPath);
                return JsonSerializer.Deserialize<UpdateSettings>(json) ?? new UpdateSettings();
            }
            catch
            {
                return new UpdateSettings();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(SettingsPath, json);
            }
            catch
            {
                // Silent fail - settings not critical
            }
        }
    }
}
