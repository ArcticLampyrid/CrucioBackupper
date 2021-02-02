using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CrucioBackupper.Crucio.Model
{
    public class SearchResult
    {
        public List<CollectionBrief> Collections { get; set; }
        public List<StoryBrief> Stories { get; set; }
        public List<TagBrief> Tags { get; set; }
        public List<UserBrief> Users { get; set; }
        [JsonPropertyName("xusers")]
        public List<UserBriefEx> UsersEx { get; set; }
        public UuidListInfo SearchStoryUuids { get; set; }
        public UuidListInfo SearchTagUuids { get; set; }
        public UuidListInfo SearchUserUuids { get; set; }
        public List<string> SubscribedTagUuids { get; set; }
        public Dictionary<string, string> TagInfoTexts { get; set; }
        public Dictionary<string, string> UserInviteCodes { get; set; }
    }
}