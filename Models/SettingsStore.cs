using System.IO;
using System.Text.Json;

namespace tpfred2.Models
{
    public class Settings
    {
        public string ApiToken { get; set; } = "";
    }

    public class SettingsStore
    {
        private readonly string _path;
        public Settings Current { get; private set; } = new();

        public SettingsStore()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "tpfred2");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "config.json");
            Load();
        }

        public void Load()
        {
            if (!File.Exists(_path)) return;
            try
            {
                Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_path)) ?? new Settings();
            }
            catch { Current = new Settings(); }
        }

        public void Save()
        {
            File.WriteAllText(_path,
                JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
