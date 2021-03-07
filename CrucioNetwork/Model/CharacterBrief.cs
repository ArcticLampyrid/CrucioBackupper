using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class CharacterBrief
    {
        [JsonPropertyName("avatar_uuid")]
        public string AvatarUuid { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("role")]
        public int Role { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}
