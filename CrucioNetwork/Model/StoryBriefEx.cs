using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class StoryBriefEx
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }
        [JsonPropertyName("like_status")]
        public int LikeStatus { get; set; }
        [JsonPropertyName("characters")]
        public List<CharacterBrief> Characters { get; set; }
    }
}