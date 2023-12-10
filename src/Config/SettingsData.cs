using System.Text.Json.Serialization;

public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{Red}[Sklepcs] ";

    [JsonPropertyName("Database")]
    public DatabaseData Database { get; set; } = new DatabaseData();
}