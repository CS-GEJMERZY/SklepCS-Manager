using System.Text.Json.Serialization;
namespace SklepCSManager;
public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = " {red}[Sklepsms] ";

    [JsonPropertyName("Database")]
    public DatabaseData Database { get; set; } = new DatabaseData();
}