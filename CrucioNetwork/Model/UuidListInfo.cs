using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class UuidListInfo
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
        [JsonPropertyName("hasmore")]
        public bool Hasmore { get; set; }
        [JsonPropertyName("list")]
        public List<string> List { get; set; }
    }
}