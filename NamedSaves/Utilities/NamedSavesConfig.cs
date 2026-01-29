using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NamedSaves.Utilities
{
    public static class NamedSavesConfig
    {
        private static readonly string ConfigDir = Path.Combine(BepInEx.Paths.ConfigPath, "NamedSaves");
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");
        private static Dictionary<string, string>? _cache;

        public static Dictionary<string, string> Load()
        {
            if (_cache != null) return _cache;
            if (!File.Exists(ConfigPath))
            {
                _cache = [];
                return _cache;
            }
            var json = File.ReadAllText(ConfigPath);
            _cache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? [];
            return _cache;
        }

        public static string? GetCustomName(string saveId)
        {
            var dict = Load();
            return dict.TryGetValue(saveId, out var name) ? name : null;
        }

        public static void SetCustomName(string saveId, string customName)
        {
            var dict = Load();
            dict[saveId] = customName;
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
        }
    }
}
