using System.Text.Json.Serialization;

namespace SklepCSManager;
public class DatabaseData
{
    [JsonPropertyName("DBHostname")]
    public string DBHostname { get; set; } = "www.yoursite.com";

    [JsonPropertyName("DBPort")]
    public uint DBPort { get; set; } = 3306;

    [JsonPropertyName("DBDatabase")]
    public string DBDatabase { get; set; } = "sklepcs_maintable";

    [JsonPropertyName("DBUser")]
    public string DBUser { get; set; } = "user_123456";

    [JsonPropertyName("DBPassword")]
    public string DBPassword { get; set; } = "passwordtodb123";
}