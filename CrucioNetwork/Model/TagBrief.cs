using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class TagBrief
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sub_tag_names")]
        public List<string> SubTagNames { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}