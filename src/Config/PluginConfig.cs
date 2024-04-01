using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Plugin
{
    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("Settings")]
        public Configs.SettingsData Settings { get; set; } = new Configs.SettingsData();

        [JsonPropertyName("Sklepcs")]
        public Configs.SklepcsData Sklepcs { get; set; } = new Configs.SklepcsData();

        [JsonPropertyName("Groups")]
        public List<Configs.SklepcsPermissionData> PermissionGroups { get; set; } = [];

        public PluginConfig()
        {
            if (PermissionGroups.Count == 0)
            {
                PermissionGroups.Add(new Configs.SklepcsPermissionData());
            }
        }
    }

}



