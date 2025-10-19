using System;
using System.IO;
using System.Text.Json;

namespace Aura
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static Settings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
                catch (Exception)
                {
                    // If deserialization fails, return default settings
                    return new Settings();
                }
            }
            return new Settings();
        }

        public static void Save(Settings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // Handle potential exceptions during file write, e.g., permission issues
            }
        }
    }
}
