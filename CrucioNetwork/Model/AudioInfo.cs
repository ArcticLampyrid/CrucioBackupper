using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class AudioInfo
    {
        [JsonPropertyName("duration")]
        public long Duration { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
