using System.Text.Json.Serialization;

public class SklepcsData
{
    [JsonPropertyName("WebsiteURL")]
    public string WebsiteURL { get; set; } = "www.sklepcs.pl/yourshop";
}
