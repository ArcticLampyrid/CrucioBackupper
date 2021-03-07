using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class MomentBrief
    {
        [JsonPropertyName("author_uuid")]
        public string AuthorUuid { get; set; }
        [JsonPropertyName("available")]
        public bool Available { get; set; }
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }
        [JsonPropertyName("create_time")]
        public long CreateTime { get; set; }
        [JsonPropertyName("c_discussion_uuid")]
        public long CDiscussionUuid { get; set; }
        [JsonPropertyName("from_tag_names")]
        public List<string> FromTagNames { get; set; }
        [JsonPropertyName("images")]
        public List<ImageInfo> Images { get; set; }
        [JsonPropertyName("editable")]
        public bool Editable { get; set; }
        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }
        [JsonPropertyName("liked")]
        public bool Liked { get; set; }
        [JsonPropertyName("story_uuid")]
        public string StoryUuid { get; set; }
        [JsonPropertyName("live_uuid")]
        public string LiveUuid { get; set; }
        [JsonPropertyName("story_comment_uuid")]
        public string StoryCommentUuid { get; set; }
        [JsonPropertyName("tag_names")]
        public List<string> TagNames { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
