using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class CollectionDetail
    {
        [JsonPropertyName("collections")]
        public List<CollectionBrief> Collections { get; set; }
        [JsonPropertyName("stories")]
        public List<StoryBrief> Stories { get; set; }
        [JsonPropertyName("story_uuids")]
        public UuidListInfo StoryUuids { get; set; }
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("discussion_author_count")]
        public int DiscussionAuthorCount { get; set; }
        [JsonPropertyName("discussion_count")]
        public int DiscussionCount { get; set; }
        [JsonPropertyName("discussion_uuids")]
        public UuidListInfo DiscussionUuids { get; set; }
        [JsonPropertyName("discussions")]
        public List<DiscussionBrief> Discussions { get; set; }
    }
}
