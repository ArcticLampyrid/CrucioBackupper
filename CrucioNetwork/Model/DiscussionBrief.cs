using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class DiscussionBrief
    {
        [JsonPropertyName("author_uuid")]
        public string AuthorUuid { get; set; }
        [JsonPropertyName("available")]
        public bool Available { get; set; }
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
        [JsonPropertyName("create_time")]
        public long CreateTime { get; set; }
        [JsonPropertyName("editable")]
        public bool Editable { get; set; }
        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }
        [JsonPropertyName("liked")]
        public bool Liked { get; set; }
        [JsonPropertyName("story_uuid")]
        public string StoryUuid { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}