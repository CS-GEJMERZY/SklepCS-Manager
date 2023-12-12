using System.Text.Json.Serialization;
namespace SklepCSManager;
public class SklepcsData
{
    [JsonPropertyName("WebsiteURL")]
    public string WebsiteURL { get; set; } = "www.sklepcs.pl/yourshop";

    [JsonPropertyName("ApiKey")]
    public string ApiKey { get; set; } = "1234567890";


    [JsonPropertyName("ServerTag")]
    public string ServerTag { get; set; } = "server1";
}
