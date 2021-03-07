using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class CollectionBrief
    {
        [JsonPropertyName("click_count")]
        public long ClickCount { get; set; }
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
        [JsonPropertyName("desc")]
        public string Desc { get; set; }
        [JsonPropertyName("is_subscribed")]
        public bool IsSubscribed { get; set; }
        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("original_statememt")]
        public string OriginalStatement { get; set; }
        [JsonPropertyName("remake_count")]
        public int RemakeCount { get; set; }
        [JsonPropertyName("share_uuid")]
        public string ShareUuid { get; set; }
        [JsonPropertyName("show_metadata")]
        public bool ShowMetadata { get; set; }
        [JsonPropertyName("story_count")]
        public int StoryCount { get; set; }
        [JsonPropertyName("tag_names")]
        public List<string> TagNames { get; set; }
        [JsonPropertyName("to_be_continued")]
        public bool ToBeContinued { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    }
}