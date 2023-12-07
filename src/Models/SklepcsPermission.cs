using System.Text.Json.Serialization;

public class SklepcsPermission
{

    [JsonPropertyName("RequiredFlags")]
    public string RequiredFlags { get; set; } = "p";


    [JsonPropertyName("Permissions")]
    public List<string> Permissions { get; set; } = new List<string> { "@sklepcs/default", "@sklepcs/2" };
}