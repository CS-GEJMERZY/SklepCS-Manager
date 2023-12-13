using System.Text.Json.Serialization;
namespace SklepCSManager;
public class SettingsData
{
    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = " {red}[Sklepsms] ";

    [JsonPropertyName("LoggingLevel")]
    public uint LoggingLevel { get; set; } = (uint)(LoggingLevelData.PurchaseSuccess | LoggingLevelData.PurchaseErrors);

    [JsonPropertyName("Database")]
    public DatabaseData Database { get; set; } = new DatabaseData();

    public bool IsLoggingLevelEnabled(LoggingLevelData level) { 
        return (LoggingLevel & (uint)level) != 0;
    }
}