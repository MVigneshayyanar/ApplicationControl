using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ApplicationControl
{

    public class AppConfig
    {
        public List<string> Whitelist { get; set; } = new List<string>();
        public List<string> Blacklist { get; set; } = new List<string>();

        private static readonly string configPath = "config.json";

        public static AppConfig Load()
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }
    }
}
