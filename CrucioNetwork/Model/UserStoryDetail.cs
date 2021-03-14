using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class UserStoryDetail
    {
        [JsonPropertyName("collections")]
        public List<CollectionBrief> Collections { get; set; }
        [JsonPropertyName("stories")]
        public List<StoryBrief> Stories { get; set; }
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("xstories")]
        public List<StoryBriefEx> Xstories { get; set; }
        [JsonPropertyName("user_story_uuids")]
        public UuidListInfo UserStoryUuids { get; set; }
        [JsonPropertyName("top_user_story_uuids")]
        public UuidListInfo TopUserStoryUuids { get; set; }
    }
}
