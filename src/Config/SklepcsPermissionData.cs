﻿using System.Text.Json.Serialization;
namespace Plugin.Configs
{
    public class SklepcsPermissionData
    {

        [JsonPropertyName("RequiredFlags")]
        public string RequiredFlags { get; set; } = "p";


        [JsonPropertyName("Permissions")]
        public List<string> Permissions { get; set; } = new List<string> { "@sklepcs/default", "@sklepcs/2" };
    }
}
