using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Config;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Settings")]
    public SettingsData Settings { get; set; } = new SettingsData();

    [JsonPropertyName("Sklepcs")]
    public SklepcsData Sklepcs { get; set; } = new SklepcsData();

    [JsonPropertyName("Groups")]
    public List<SklepcsPermission> PermissionGroups { get; set; } = new List<SklepcsPermission>();

    public PluginConfig()
    {
        if (PermissionGroups.Count == 0)
        {
            PermissionGroups.Add(new SklepcsPermission());
        }
    }
}


