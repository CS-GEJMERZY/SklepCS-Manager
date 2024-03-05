using System.Text.Json.Serialization;
namespace Plugin.Configs
{
    public class SklepcsData
    {
        [JsonPropertyName("WebFeaturesEnabled")]
        public bool WebFeaturesEnabled { get; set; } = true;

        [JsonPropertyName("WebsiteURL")]
        public string WebsiteURL { get; set; } = "www.sklepcs.pl/yourshop";

        [JsonPropertyName("ApiKey")]
        public string ApiKey { get; set; } = "1234567890";

        [JsonPropertyName("ServerTag")]
        public string ServerTag { get; set; } = "server1";
    }

}
