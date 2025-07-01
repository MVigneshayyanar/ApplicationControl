using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ApplicationControl
{
    public class AppConfig
    {
        public List<AppInfo> Whitelist { get; set; } = new();
        public List<AppInfo> Blacklist { get; set; } = new();

        private static readonly string configPath = "config.json";

        public static AppConfig Load()
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }
    }

}
