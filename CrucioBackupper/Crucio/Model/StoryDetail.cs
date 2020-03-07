using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioBackupper.Crucio.Model
{
    public class StoryDetail
    {
        public List<CollectionBrief> Collections { get; set; }
        public List<StoryBrief> Stories { get; set; }
        public List<UserBrief> Users { get; set; }
        public UuidListInfo DiscussionUuids { get; set; }
        public List<DiscussionBrief> Discussions { get; set; }
        [JsonProperty("xusers")]
        public List<UserBriefEx> UsersEx { get; set; }
        [JsonProperty("xstories")]
        public List<StoryBriefEx> StoriesEx { get; set; }
    }
}
