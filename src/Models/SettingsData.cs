using System.Text.Json.Serialization;

public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{Red}[Sklepcs] ";

    [JsonPropertyName("ServerTag")]
    public string ServerTag { get; set; } = "server1";


    [JsonPropertyName("Database")]
    public DatabaseData Database { get; set; } = new DatabaseData();


}