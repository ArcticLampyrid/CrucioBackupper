using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class StoryDetail
    {
        [JsonPropertyName("collections")]
        public List<CollectionBrief> Collections { get; set; }
        [JsonPropertyName("stories")]
        public List<StoryBrief> Stories { get; set; }
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("discussion_uuids")]
        public UuidListInfo DiscussionUuids { get; set; }
        [JsonPropertyName("discussions")]
        public List<DiscussionBrief> Discussions { get; set; }
        [JsonPropertyName("xusers")]
        public List<UserBriefEx> UsersEx { get; set; }
        [JsonPropertyName("xstories")]
        public List<StoryBriefEx> StoriesEx { get; set; }
    }
}
