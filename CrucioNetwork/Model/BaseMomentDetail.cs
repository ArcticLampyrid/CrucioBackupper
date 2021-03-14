using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioNetwork.Model
{
    public class BaseMomentDetail
    {
        [JsonPropertyName("collections")]
        public List<CollectionBrief> Collections { get; set; }
        [JsonPropertyName("moments")]
        public List<MomentBrief> Moments { get; set; }
        [JsonPropertyName("stories")]
        public List<StoryBrief> Stories { get; set; }
        [JsonPropertyName("users")]
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("xstories")]
        public List<StoryBriefEx> StoriesEx { get; set; }
    }
}
